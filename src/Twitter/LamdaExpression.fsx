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

// test
let userStatusQuery =
    query{
        for tweet in ctx.Status do
        where (tweet.Type = StatusType.User)
        select tweet        
    }

let kimskStatusQuery =
    query {
        for tweet in userStatusQuery do
        where (tweet.ScreenName = "kimsk")
        select tweet
    }

let nashdotnetStatusQuery = userStatusQuery.Where(fun t -> t.ScreenName = "nashdotnet")
//    query {
//        for tweet in userStatusQuery do
//        where (tweet.ScreenName = "nashdotnet")
//        select tweet
//    }


kimskStatusQuery.First().Text

nashdotnetStatusQuery.First().Text

//let userType = Func<_,_>(fun (t:Status) -> t.Type = StatusType.User)

let userType = <@ Func<_,_>(fun (t:Status) -> t.Type = StatusType.User) @>

open Microsoft.FSharp.Linq.RuntimeHelpers
let userTypeExpr = LeafExpressionConverter.QuotationToLambdaExpression(userType)

let screenName name = 
    LeafExpressionConverter
        .QuotationToLambdaExpression(<@ Func<_,_>(fun (t:Status) -> t.ScreenName = name) @>)

ctx.Status
    .Where(userTypeExpr)
    .Where(screenName "kimsk")
    |> Seq.map (fun t -> t.Text)
    |> Array.ofSeq
