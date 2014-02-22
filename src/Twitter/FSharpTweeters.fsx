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
