module Search

open System
open System.Linq
open LinqToTwitter
open TwitterContext

let getSearchResult q num = 
    query {
        for searchResult in ctx.Search do
        where (searchResult.Type = SearchType.Search)
        where (searchResult.Query = q)
        where (searchResult.Count = num)     
        select searchResult    
        exactlyOne        
    }

let getSearchResultWithMaxId q num maxId = 
    query {
        for searchResult in ctx.Search do
        where (searchResult.Type = SearchType.Search)
        where (searchResult.Query = q)
        where (searchResult.Count = num)     
        where (searchResult.MaxID = maxId)
        select searchResult    
        exactlyOne        
    }

let getStatuses q num batches =
    let s2ul (s:string) = Convert.ToUInt64(s)

    let statuses = (getSearchResult q num).Statuses |> List.ofSeq

    let rec getSearchResultsRec q batches (statuses:list<Status>) =
        match batches with
        | 0 -> []
        | _ ->
            let maxId =  (statuses.Last().StatusID |> s2ul) - 1UL
            getSearchResultsRec q (batches-1) statuses @ ((getSearchResultWithMaxId q num maxId).Statuses |> List.ofSeq)            
                        
    statuses @ getSearchResultsRec q (batches-1) statuses