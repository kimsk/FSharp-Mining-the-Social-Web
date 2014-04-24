(*** hide ***)
(* BUILD (Ctrl+Shift+B) the project to restore NuGet packages first! *)
#I ".."
#load "packages/FsLab.0.0.13-beta/FsLab.fsx"
#r @"..\src\lib\LinqToTwitter.dll"
#I @"..\..\src\twitter\"
#load "TwitterCredential.fs"
#load "TwitterContext.fs"
#load "Trends.fs"
#load "Search.fs"
open System
open System.Linq
open LinqToTwitter
open TwitterContext
open Deedle
open FSharp.Charting
(**

﻿Mining Twitter
==============
#### Concepts
- Basic frequency distribution : most common
- Lexical diversity
- Average words
- Ploting log-log graph for frequencies of words 
- Histograms

>The first chapter is available [here](http://nbviewer.ipython.org/github/ptwobrussell/Mining-the-Social-Web-2nd-Edition/blob/master/ipynb/__Chapter%201%20-%20Mining%20Twitter%20%28Full-Text%20Sampler%29.ipynb).


####Linq To Twitter
#####Trends
*)
let getTrends trend_id = 
        query {
            for trend in ctx.Trends do
            where (trend.Type = TrendType.Place)
            where (trend.WoeID = trend_id)
            select trend
        }

(**
#####Seach
*)
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
    let s2ul (s:string) = Convert.ToUInt64(s)

    let getStatuses q maxId =
        (getSearchResultWithMaxId q num maxId).Statuses |> List.ofSeq |> List.rev

    let combinedStatuses (acc:Status list) _ =
        let maxId =  
            if acc = [] then UInt64.MaxValue
            else (acc |> List.head |> (fun s -> s.StatusID)  |> s2ul) - 1UL
        (getStatuses q maxId) @ acc

    [0..batches] 
        |> List.fold combinedStatuses []

(** 
####Example 2. Retrieving Trends 
*)
let world_woe_id = 1
let us_woe_id = 23424977  

let worldTrends = Trends.getTrends world_woe_id
let usTrends = Trends.getTrends us_woe_id

(** 
####Example 4. Computing the intersection of two sets of trends 
*)
let commonTrends = usTrends |> worldTrends.Intersect

(** 
####Example 5. Collecting search results 
*)
let q = "#fsharp"

// get five batches, 100 tweets each
let statuses = Search.getStatuses q 100 5

(** 
*To be continued...*
*)