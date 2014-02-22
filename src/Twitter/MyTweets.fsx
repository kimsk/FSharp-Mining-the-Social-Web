#r @"..\lib\LinqToTwitter.dll"
#r "System.Windows.Forms.DataVisualization.dll"
#r @"..\lib\FSharp.Charting.dll"
#load "TwitterCredential.fs"
#load "TwitterContext.fs"
#load "Trends.fs"
#load "Search.fs"
#load "PrettyTable.fs"

open LinqToTwitter
open System
open System.Linq
open TwitterContext
open FSharp.Charting

let myTweets = Search.getTweetsByScreenName "kimsk" 10

myTweets.Count()

let isFSharpTweet (tweet : Status) = 
    tweet.Entities.HashTagEntities.Select(fun s -> s.Tag.ToLower()).Contains("fsharp") 
    || tweet.Text.ToLower().Contains("f#")

let fSharpTweets, nonFSharpTweets = 
    myTweets
    |> List.ofSeq
    |> List.partition isFSharpTweet

nonFSharpTweets
|> Seq.map (fun t -> t.CreatedAt, t.Text)
|> Seq.sortBy fst
|> PrettyTable.show "Non F# Tweets"
fSharpTweets
|> Seq.map (fun t -> t.CreatedAt, t.Text)
|> Seq.sortBy fst
|> PrettyTable.show "F# Tweets"

let epochTime = (new DateTime(1970, 1, 1)).ToLocalTime()

let countFSharpTweets = 
    fSharpTweets
    |> Seq.sortBy (fun s -> (s.CreatedAt - epochTime))
    |> Seq.groupBy (fun s -> (s.CreatedAt.Month, s.CreatedAt.Year))
    |> Seq.map (fun (k, v) -> (sprintf "%d/%d" (fst (k)) (snd (k))), v.Count())
    |> Seq.take 12
    |> Array.ofSeq

let countNonFSharpTweets = 
    nonFSharpTweets
    |> Seq.sortBy (fun s -> (s.CreatedAt - epochTime))
    |> Seq.groupBy (fun s -> (s.CreatedAt.Month, s.CreatedAt.Year))
    |> Seq.map (fun (k, v) -> (sprintf "%d/%d" (fst (k)) (snd (k))), v.Count())
    |> Seq.take 12
    |> Array.ofSeq

let chart = 
    [ Chart.Line(countFSharpTweets, Name = "Tweets with #fsharp or F#")
      Chart.Line(countNonFSharpTweets, Name = "Others") ]
    |> Chart.Combine
    |> Chart.WithLegend(Enabled=true, Docking=ChartTypes.Docking.Left)
    |> Chart.WithTitle("F# vs. Non-F# tweets")

chart.ShowChart()


