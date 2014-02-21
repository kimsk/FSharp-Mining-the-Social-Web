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

    let getStatuses q maxId =
        (getSearchResultWithMaxId q num maxId).Statuses |> List.ofSeq |> List.rev

    let combineStatuses (acc:Status list) _ =
        let maxId =  
            if acc = [] then UInt64.MaxValue
            else (acc |> List.head |> (fun s -> s.StatusID)  |> s2ul) - 1UL
        (getStatuses q maxId) @ acc

    [0..batches] 
        |> List.fold combineStatuses []