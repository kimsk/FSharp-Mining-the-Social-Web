#r @"..\lib\LinqToTwitter.dll"

#load @"TwitterCredential.fs"
#load @"TwitterContext.fs"
#load @"Trends.fs"
#load @"Search.fs"
#load @"PrettyTable.fs"
open LinqToTwitter
open System
open System.Linq
open TwitterContext

// Example 2. Retrieving trends
let world_woe_id = 1
let us_woe_id = 23424977  

let worldTrends = Trends.getTrends world_woe_id
let usTrends = Trends.getTrends us_woe_id
PrettyTable.show "World Trends" (worldTrends)
PrettyTable.show "US Trends" (usTrends)

// Example 4. Computing the intersection of two sets of trends
let commonTrends = usTrends |> worldTrends.Intersect
PrettyTable.show "Common Trends" (commonTrends)


// Example 5. Collecting search results
let q = "#fsharp"

// get five batches, 100 tweets each
let statuses = Search.getStatuses q 100 5

statuses    
    |> Seq.distinctBy (fun s -> (s.Text, s.ScreenName)) 
    |> Seq.sortBy (fun s -> -s.RetweetCount)
    |> Seq.mapi (fun i s -> i+1, s.StatusID, s.User.Identifier.ScreenName, s.Text, s.CreatedAt, s.RetweetCount)    
    |> PrettyTable.show "Five batches of results"

// example 6.
let usersMentioned = 
    [
        for status in statuses do
            for userMentioned in status.Entities.UserMentionEntities ->
                userMentioned.ScreenName
    ]

let statusTexts = [ for status in statuses -> status.Text ]
let screenNames = 
    [
        for status in statuses do
            for userMentioned in status.Entities.UserMentionEntities ->
                userMentioned.ScreenName
    ]
let hashtags = 
    [
        for status in statuses do
            for hashtag in status.Entities.HashTagEntities ->
                hashtag.Tag
    ]

let words = statusTexts |> List.collect (fun s -> (s.Split() |> List.ofArray))

(statusTexts |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Status Texts" 
(screenNames |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Screen Names"
(hashtags |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Hashtags"
(words |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Words"

// Example 7. Creating a basic frequency distribution
// Example 8. Pretty table
let getMostCommon (tokens:seq<string>) count =
    let tokensCount = tokens |> Seq.length
    let len = if tokensCount <= count then tokensCount else count
    query {
        for t in tokens do
        groupBy t into g
        select (g.Key, g.Count())
    }  
        |> Seq.sortBy (fun x -> -snd(x))
        |> Seq.take len
        |> Seq.map (fun x -> 
            { 
                PrettyTable.CountRow.Name = fst(x)
                PrettyTable.CountRow.Count = snd(x)
            }
        )
        |> Array.ofSeq

(getMostCommon words 10) |> PrettyTable.showCounts "Words"
(getMostCommon screenNames 10) |> PrettyTable.showCounts "Screen Names"
(getMostCommon hashtags 10) |> PrettyTable.showCounts "Hashtags"

// Example 9. Calculating lexical diversity for tweets
let lexicalDiversity (tokens:seq<string>) =
        1.0 * (tokens |> Set.ofSeq |> Seq.length |> float)/(tokens |> Seq.length |>float)

(*
// 1-to-1 mapping to Python code
let averageWords (statusTexts:seq<string>) =
    let totalWords =           
        statusTexts // List of tweets
        |> Seq.map (fun s -> s.Split()) // List of Array of words
        |> Seq.map (fun s -> s.Length) // List of Number of words
        |> Seq.sum
        |> float 

    1.0 * totalWords / (statusTexts |> Seq.length |> float)
*)
// Computing the average number of words per tweet
let averageWords (statusTexts:seq<string>) =
    statusTexts 
        |> Seq.map (fun s -> s.Split()) 
        |> Seq.map (fun s -> s.Length |> float) 
        |> Seq.average

lexicalDiversity words
lexicalDiversity screenNames
lexicalDiversity hashtags
averageWords statusTexts

// Example 10. Finding the most popular retweets
type RetweetTable = { Count:int; Handle:string; Text:string}
let retweets = 
    statuses         
        |> Seq.filter (fun s -> s.RetweetCount > 0)
        |> Seq.distinctBy (fun s -> s.Text)
        |> Seq.sortBy (fun s -> -s.RetweetCount)                                
        |> Seq.map (fun s -> (s.RetweetCount, s.User.Identifier.ScreenName, s.Text))                

(retweets |> Seq.map (fun (c,n,t) -> {Count=c; Handle=n; Text=t}))
    |> PrettyTable.show "Most Popular Retweets" 


// Example 11. Looking up users who have retweeted a status
let mostPopularStatusId = 
    (statuses         
        |> Seq.sortBy (fun s -> -s.RetweetCount)    
        |> Seq.map (fun s -> s.RetweetedStatus.StatusID)).First()

let retweeters = 
    let users = 
        (query {
            for tweet in ctx.Status do
            where (tweet.Type = StatusType.Retweeters)
            where (tweet.ID = mostPopularStatusId)
            select tweet
            exactlyOne
        }).Users
        |> Seq.map (fun u -> u.ToString())
        |> Seq.reduce (fun acc u -> acc + ", " + u)
    
    query{
        for user in ctx.User do
        where (user.Type = UserType.Lookup)
        where (user.UserID = users)
        select user.Identifier.ScreenName
    } |> Array.ofSeq |> Array.mapi (fun i u -> i+1, u)

retweeters |> PrettyTable.show "Retweeters"


// Example 12. Plotting frequencies of words
#r "System.Windows.Forms.DataVisualization.dll"
#r @"..\lib\FSharp.Charting.dll"
open FSharp.Charting

words |> PrettyTable.showListOfStrings "Words"

let wordCounts = 
    query {
        for word in words do
        groupBy word into g
        select (g.Key, g.Count())
    } 
    
let idxFreqLogLog =  
    wordCounts
        |> Seq.sortBy (fun x -> -snd(x))
        |> Seq.mapi (fun i x ->  log(float (i+1)), log(float <| snd(x)))        

idxFreqLogLog |> PrettyTable.show "Frequencies of Words"

(Chart.Line(idxFreqLogLog, Name="Example 12", Title="Frequency Data", YTitle="Freq", XTitle="Word Rank")).ShowChart()

// Example 13. Generating histograms of words, screen names, and hashtags
let getHistogram items =
    items 
        |> Seq.groupBy (snd) 
        |> Seq.map (fun (k,s) -> k, s.Count())
        |> Seq.sortBy (fst)

let wordsHistogram = getHistogram wordCounts |> Seq.take 20
let screenNamesHistogram = 
    query{
        for screenName in screenNames do
        groupBy screenName into g
        select (g.Key, g.Count())
    } |> getHistogram |> Seq.take 12

let hashtagsHistogram = 
    query{
        for hashtag in hashtags do
        groupBy hashtag into g
        select (g.Key, g.Count())
    } |> getHistogram |> Seq.take 12

let yTitle = "Number of items in bin"
let xTitle = "Bins (number of times an items appeared)"
Chart.Column(wordsHistogram, Name="Words", YTitle=yTitle, XTitle=xTitle)
    .ShowChart()
Chart.Column(screenNamesHistogram, Name="Screen Names", YTitle=yTitle, XTitle=xTitle).ShowChart()
Chart.Column(hashtagsHistogram, Name="Hashtags", YTitle=yTitle, XTitle=xTitle).ShowChart()


// Example 14. Generating a histogram of retweet counts
let retweetsHistogram = 
    retweets     
    |> Seq.groupBy (fun (c,_,_) -> c)
    |> Seq.map (fun (k,s) -> k, s.Count())
    |> Seq.sortBy (fst)

Chart.Column(retweetsHistogram, Name="Retweets", YTitle=yTitle, XTitle=xTitle).ShowChart()
