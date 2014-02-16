#r @"packages\linqtotwitter.2.1.11\lib\net45\LinqToTwitter.dll"

#load @"TwitterCredential.fs"
#load @"TwitterContext.fs"
#load @"Trends.fs"
#load @"Search.fs"
#load @"PrettyTable.fs"
open LinqToTwitter
open System.Linq

// Example 2. Retrieving trends
let world_woe_id = 1
let us_woe_id = 23424977  

let worldTrends = Trends.getTrends world_woe_id
let usTrends = Trends.getTrends us_woe_id
PrettyTable.show "World Trends" (worldTrends |> Array.ofSeq)
PrettyTable.show "US Trends" (usTrends |> Array.ofSeq)

// Example 4. Computing the intersection of two sets of trends
let commonTrends = usTrends |> worldTrends.Intersect
PrettyTable.show "Common Trends" (commonTrends |> Array.ofSeq)


// Example 5. Collecting search results
let q = "#AnchorDown"
let statuses = (Search.query q 100).Statuses
statuses.First().RetweetCount
statuses.First().RetweetedStatus

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
    query {
        for t in tokens do
        groupBy t into g
        select (g.Key, g.Count())
    }  
        |> Seq.sortBy (fun x -> -snd(x))
        |> Seq.take count
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