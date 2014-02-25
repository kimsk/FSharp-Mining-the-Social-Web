module Status

open System
open System.Linq
open LinqToTwitter
open TwitterContext
open Microsoft.FSharp.Linq.RuntimeHelpers

let buildSearchQuery statusType (filters:(Status->bool) list) = 
    let status = ctx.Status.Where(fun s -> s.Type = statusType)

    let expr f = 
        LeafExpressionConverter
            .QuotationToLambdaExpression(<@ Func<_, _>(f)@>)
        
    filters |> List.fold (fun (q:IQueryable<Status>) f -> q.Where(expr f)) status

[
    (fun (s:Status) -> s.ScreenName = "kimsk")
    (fun s -> s.Count = 10)
] 
    |> buildSearchQuery StatusType.User 
    |> Array.ofSeq

buildSearchQuery StatusType.User []
    |> Array.ofSeq