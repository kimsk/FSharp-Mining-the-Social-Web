module Status

open System
open System.Linq
open LinqToTwitter
open TwitterContext
open Microsoft.FSharp.Linq.RuntimeHelpers
open Microsoft.FSharp.Quotations

let expr f = 
    let f' = <@ Func<_,_>(f) @>
    LeafExpressionConverter
        .QuotationToLambdaExpression(f')

let buildSearchQuery statusType filters = 
    let status = ctx.Status.Where(fun s -> s.Type = statusType)    
        
    (filters 
        |> List.fold (fun (q:IQueryable<Status>) f -> q.Where(expr f)) status)

let filters = 
    [
        (fun (s:Status) -> s.ScreenName = "kimsk")
        (fun s -> s.Count = 1)
    ] 

filters
    |> buildSearchQuery StatusType.User 
    |> Array.ofSeq |> ignore

let expr' = LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<Status, bool>(fun t -> t.Count = 1)@>)

(buildSearchQuery StatusType.User [])
    //.Where(expr filters.Head)
    //.Where(expr (fun (t:Status) -> t.Count = 1))
    //.Where(LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<Status, bool>(fun t -> t.Count = 1)@>))
    .Where(LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<Status, bool>(filters.Head)@>))
    //.Where(expr')
    |> Array.ofSeq |> ignore

let filters' = 
    [
        LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<_,_>(fun (s:Status) -> s.ScreenName = "kimsk") @>)        
        LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<_,_>(fun s -> s.Count = 1) @>)
    ] 

let filters'' = filters |> List.map (fun f -> LeafExpressionConverter.QuotationToLambdaExpression(<@ Func<Status, bool>(f)@>))

ctx.Status.Where(fun s -> s.Type = StatusType.User)
    .Where(filters'.Head) |> Array.ofSeq