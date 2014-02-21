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
let statuses = Search.getStatuses q 100 20

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
let hashTags = 
    [
        for status in statuses do
            for hashTag in status.Entities.HashTagEntities ->
                hashTag.Tag
    ]

let words = statusTexts |> List.collect (fun s -> (s.Split() |> List.ofArray))

(statusTexts |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Status Texts" 
(screenNames |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Screen Names"
(hashTags |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Hash Tags"
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

(getMostCommon words 10) |> PrettyTable.showCounts "Most Common Words"
(getMostCommon screenNames 10) |> PrettyTable.showCounts "Most Common Screen Names"
(getMostCommon hashTags 10) |> PrettyTable.showCounts "Most Common Hash Tags"

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
lexicalDiversity hashTags
averageWords statusTexts

// Example 10. Finding the most popular retweets
let retweets = 
    statuses         
        |> Seq.filter (fun s -> s.RetweetCount > 0)
        |> Seq.distinctBy (fun s -> s.Text)
        |> Seq.sortBy (fun s -> -s.RetweetCount)                                
        |> Seq.map (fun s -> (s.StatusID, s.RetweetCount, s.Text, s.User.Identifier.ScreenName))                

PrettyTable.show "Most Popular Retweets" retweets


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
words |> PrettyTable.showListOfStrings "Words"

let countWords = 
    query {
        for word in words do
        groupBy word into g
        select (g.Key, g.Count())
    }  
        |> Seq.sortBy (fun x -> -snd(x))

countWords |> PrettyTable.show "Word Counts"
