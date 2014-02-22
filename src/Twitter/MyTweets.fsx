#r @"..\lib\LinqToTwitter.dll"
#r "System.Windows.Forms.DataVisualization.dll"
#r @"..\lib\FSharp.Charting.dll"

#load @"TwitterCredential.fs"
#load @"TwitterContext.fs"
#load @"Trends.fs"
#load @"Search.fs"
#load @"PrettyTable.fs"
open LinqToTwitter
open System
open System.Linq
open TwitterContext
open FSharp.Charting

let fSharpTweets = Search.getStatuses "#FSharp" 100 5

fSharpTweets 
    |> Seq.groupBy (fun t -> t.User.Identifier.ScreenName) 
    |> Seq.map (fun (k,s) -> {PrettyTable.CountRow.Name=k; PrettyTable.CountRow.Count=s.Count()})
    |> Seq.sortBy (fun c -> -c.Count) 
    |> Array.ofSeq   
    |> PrettyTable.showCounts "F# Tweeters"

let kimskTweets = 
    query{
        for tweet in ctx.Status do
        where (tweet.Type = StatusType.User)
        where (tweet.ScreenName = "kimsk")
        where (tweet.Count = 10)        
        select tweet
    }

kimskTweets.Last().StatusID

let getMyTweets batches = 
    let s2ul (s : string) = Convert.ToUInt64(s)
    
    let getTweets maxId = 
        let tweets = 
            if maxId = UInt64.MaxValue then
                query{
                    for tweet in ctx.Status do
                        where (tweet.Type = StatusType.User)
                        where (tweet.ScreenName = "kimsk")
                        where (tweet.Count = 200)                        
                        select tweet
                }
            else
                query{
                    for tweet in ctx.Status do
                        where (tweet.Type = StatusType.User)
                        where (tweet.ScreenName = "kimsk")
                        where (tweet.Count = 200)
                        where (tweet.MaxID = maxId)
                        select tweet
                }

        tweets
        |> List.ofSeq
        |> List.rev
    
    let getAllTweets (acc : Status list) _ = 
        let maxId = 
            if acc = [] then UInt64.MaxValue
            else 
                (acc
                 |> List.head
                 |> (fun s -> s.StatusID)
                 |> s2ul) - 1UL
        (getTweets maxId) @ acc
    
    [ 0..batches ] |> List.fold getAllTweets []

let myTweets = getMyTweets 10

myTweets.Count()

let tweets, nonFSharpTweets = 
    myTweets
    |> List.ofSeq
    |> List.partition (fun t -> t.Entities.HashTagEntities.Select(fun s-> s.Tag.ToLower()).Contains("fsharp"))

let epochTime = (new DateTime(1970, 1, 1)).ToLocalTime()
let countTweets = 
    tweets 
    |> Seq.sortBy (fun s -> (s.CreatedAt - epochTime))        
    |> Seq.groupBy (fun s -> (s.CreatedAt.Month, s.CreatedAt.Year))   
    |> Seq.map (fun (k,v) -> (sprintf "%d/%d" (fst(k)) (snd(k))), v.Count())
    |> Array.ofSeq
    

let countNonFSharpTweets = 
    nonFSharpTweets 
    |> Seq.sortBy (fun s -> -(s.CreatedAt - epochTime))        
    |> Seq.groupBy (fun s -> (s.CreatedAt.Month, s.CreatedAt.Year))   
    |> Seq.map (fun (k,v) -> (sprintf "%d/%d" (fst(k)) (snd(k))), v.Count())
    |> Array.ofSeq

Chart.Combine(
    [
        Chart.Line(countTweets, Name="Tweets with #fsharp")
        Chart.Line(countNonFSharpTweets, Name="Others")
    ]).ShowChart()

