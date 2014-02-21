#Mining Twitter


>The first chapter is available [here](http://nbviewer.ipython.org/github/ptwobrussell/Mining-the-Social-Web-2nd-Edition/blob/master/ipynb/__Chapter%201%20-%20Mining%20Twitter%20%28Full-Text%20Sampler%29.ipynb).

####Linq To Twitter
#####Trends
```fsharp
let getTrends trend_id = 
        query {
            for trend in ctx.Trends do
            where (trend.Type = TrendType.Place)
            where (trend.WoeID = trend_id)
            select trend
        }
```
#####Seach
```fsharp
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
```


####Example 2. Retrieving Trends
```fsharp
let world_woe_id = 1
let us_woe_id = 23424977  

let worldTrends = Trends.getTrends world_woe_id
let usTrends = Trends.getTrends us_woe_id
```

####Example 4. Computing the intersection of two sets of trends
```fsharp
let commonTrends = usTrends |> worldTrends.Intersect
```

####Example 5. Collecting search results
```fsharp
let q = "#fsharp"

// get five batches, 100 tweets each
let statuses = Search.getStatuses q 100 5

statuses    
    |> Seq.distinctBy (fun s -> (s.Text, s.ScreenName)) 
    |> Seq.sortBy (fun s -> -s.RetweetCount)
    |> Seq.map (fun s -> s.StatusID, s.User.Identifier.ScreenName, s.Text, s.CreatedAt, s.RetweetCount)     
    |> Seq.mapi (fun i (s, n, t, c, r) -> i+1, s, n, t, c, r)
    |> Array.ofSeq    
    |> PrettyTable.show "Five batches of results"
```

####Example 6. Extracting text, screen names, and hashtags from tweets
```fsharp
let usersMentioned = 
    [
        for status in statuses do
            for userMentioned in status.Entities.UserMentionEntities ->
                userMentioned.ScreenName
    ]

let statusTexts = [ for status in statuses -> status.Text ]
let screenNames = 
    [
        for status in statuses do
            for userMentioned in status.Entities.UserMentionEntities ->
                userMentioned.ScreenName
    ]
let hashTags = 
    [
        for status in statuses do
            for hashTag in status.Entities.HashTagEntities ->
                hashTag.Tag
    ]

let words = statusTexts |> List.collect (fun s -> (s.Split() |> List.ofArray))

(statusTexts |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Status Texts" 
(screenNames |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Screen Names"
(hashTags |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Hash Tags"
(words |> Array.ofSeq).[..5] |> PrettyTable.showListOfStrings "Words"
```

####Example 7. Creating a basic frequency distribution
```fsharp
let getMostCommon (tokens:seq<string>) count =
    let tokensCount = tokens |> Seq.length
    let len = if tokensCount <= count then tokensCount else count
    query {
        for t in tokens do
        groupBy t into g
        select (g.Key, g.Count())
    }  
        |> Seq.sortBy (fun x -> -snd(x))
        |> Seq.take len
        |> Seq.map (fun x -> 
            { 
                PrettyTable.CountRow.Name = fst(x)
                PrettyTable.CountRow.Count = snd(x)
            }
        )
        |> Array.ofSeq

(getMostCommon words 10) |> PrettyTable.showCounts "Most Common Words"
(getMostCommon screenNames 10) |> PrettyTable.showCounts "Most Common Screen Names"
(getMostCommon hashTags 10) |> PrettyTable.showCounts "Most Common Hash Tags"
```

####Example 8. F# PrettyTable using DataGrid
```fsharp
module PrettyTable

open System.Drawing
open System.Windows.Forms

let buildForm title =
    let form = new Form(Visible = true, Text = title,
                        TopMost = true, Size = Size(600,600))

    let data = new DataGridView(Dock = DockStyle.Fill, Text = "F#",
                                Font = new Font("Lucida Console",12.0f),
                                ForeColor = Color.DarkBlue)
 
    form.Controls.Add(data)
    data

let show title dataSource =
    let data = buildForm title
    data.DataSource <- dataSource
```

####Example 9. Calculating lexical diversity for tweets
```fsharp
let lexicalDiversity (tokens:seq<string>) =
        1.0 * (tokens |> Set.ofSeq |> Seq.length |> float)/(tokens |> Seq.length |>float)


// Computing the average number of words per tweet
let averageWords (statusTexts:seq<string>) =
    statusTexts 
        |> Seq.map (fun s -> s.Split()) 
        |> Seq.map (fun s -> s.Length |> float) 
        |> Seq.average

lexicalDiversity words
lexicalDiversity screenNames
lexicalDiversity hashTags
averageWords statusTexts
```

####Example 10. Finding the most popular retweets
```fsharp
let retweets = 
    statuses         
        |> Seq.filter (fun s -> s.RetweetCount > 0)                                
        |> Seq.map (fun s -> (s.RetweetCount, s.Text, s.User.Identifier.ScreenName))        
        |> Seq.distinctBy (fun (_,t,_) -> t)
        |> Seq.sortBy (fun (c,_,_) -> -c)
        |> Array.ofSeq

PrettyTable.show "Most Popular Retweets" retweets
```

####Example 11. Looking up users who have retweeted a status
```fsharp
let mostPopularStatusId = 
    (statuses         
        |> Seq.sortBy (fun s -> -s.RetweetCount)    
        |> Seq.map (fun s -> s.RetweetedStatus.StatusID)).First()

let retweeters = 
    let users = 
        (query {
            for tweet in ctx.Status do
            where (tweet.Type = StatusType.Retweeters)
            where (tweet.ID = mostPopularStatusId)
            select tweet
            exactlyOne
        }).Users
        |> Seq.map (fun u -> u.ToString())
        |> Seq.reduce (fun acc u -> acc + ", " + u)
    
    query{
        for user in ctx.User do
        where (user.Type = UserType.Lookup)
        where (user.UserID = users)
        select user.Identifier.ScreenName
    } |> Array.ofSeq |> Array.mapi (fun i u -> i+1, u)

retweeters |> PrettyTable.show "Retweeters"
```

##Credits
* [Don Syme](https://twitter.com/dsyme) and Microsoft for F# :-) and his tips for [Visualizing Data in a Grid](http://blogs.msdn.com/b/dsyme/archive/2010/01/08/f-interactive-tips-and-tricks-visualizing-data-in-a-grid.aspx)
* [Joe Mayo](https://twitter.com/JoeMayo) for his awesome [LINQ to Twitter](https://linqtotwitter.codeplex.com/)



