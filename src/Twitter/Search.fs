module Search

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
    