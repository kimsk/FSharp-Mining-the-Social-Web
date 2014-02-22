module Search

open System
open System.Linq
open LinqToTwitter
open TwitterContext

let s2ul (s : string) = Convert.ToUInt64(s)

// get search result
let getSearchResult q num = 
    query { 
        for searchResult in ctx.Search do
            where (searchResult.Type = SearchType.Search)
            where (searchResult.Query = q)
            where (searchResult.Count = num)
            select searchResult
            exactlyOne
    }

// get search result with specific maxId
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

// get statuses from number of batches
let getStatuses q num batches = 
    
    let getStatuses q maxId = 
        (getSearchResultWithMaxId q num maxId).Statuses
        |> List.ofSeq
        |> List.rev
    
    let combinedStatuses (acc : Status list) _ = 
        let maxId = 
            if acc = [] then UInt64.MaxValue
            else 
                (acc
                 |> List.head
                 |> (fun s -> s.StatusID)
                 |> s2ul) - 1UL
        (getStatuses q maxId) @ acc
    
    [ 0..batches ] |> List.fold combinedStatuses []

// get tweets by screen name from number of batches
let getTweetsByScreenName screenName batches = 
        
    let getTweets maxId = 
        let tweets = 
            if maxId = UInt64.MaxValue then
                query{
                    for tweet in ctx.Status do
                        where (tweet.Type = StatusType.User)
                        where (tweet.ScreenName = screenName)
                        where (tweet.Count = 200)                        
                        select tweet
                }
            else
                query{
                    for tweet in ctx.Status do
                        where (tweet.Type = StatusType.User)
                        where (tweet.ScreenName = screenName)
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