#Mining Twitter


>The first chapter is available [here](http://nbviewer.ipython.org/github/ptwobrussell/Mining-the-Social-Web-2nd-Edition/blob/master/ipynb/__Chapter%201%20-%20Mining%20Twitter%20%28Full-Text%20Sampler%29.ipynb).

####Linq To Twitter
#####Trends
```
let getTrends trend_id = 
        query {
            for trend in ctx.Trends do
            where (trend.Type = TrendType.Place)
            where (trend.WoeID = trend_id)
            select trend
        }
```
#####Seach
```
let query q num = 
    query {
        for searchResult in ctx.Search do
        where (searchResult.Type = SearchType.Search)
        where (searchResult.Query = q)
        where (searchResult.Count = num)     
        select searchResult    
        exactlyOne        
    }
```


####Example 2. Retrieving Trends
```
let world_woe_id = 1
let us_woe_id = 23424977  

let worldTrends = Trends.getTrends world_woe_id
let usTrends = Trends.getTrends us_woe_id
```

####Example 4. Computing the intersection of two sets of trends
```
let commonTrends = usTrends |> worldTrends.Intersect
```

####Example 5. Collecting search results
```
let q = "#FSharp"
let statuses = (Search.query q 100).Statuses
statuses.First().RetweetCount
statuses.First().RetweetedStatus
```

####Example 6. Extracting text, screen names, and hashtags from tweets
```
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
```
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
```
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
```
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
```
let retweets = 
    statuses         
        |> Seq.filter (fun s -> s.RetweetCount > 0)                                
        |> Seq.map (fun s -> (s.RetweetCount, s.Text, s.User.Identifier.ScreenName))        
        |> Seq.distinctBy (fun (_,t,_) -> t)
        |> Seq.sortBy (fun (c,_,_) -> -c)
        |> Array.ofSeq

PrettyTable.show "Most Popular Retweets" retweets
```

##Credits
* [Don Syme](https://twitter.com/dsyme) for F# :-) and his tips for [Visualizing Data in a Grid](http://blogs.msdn.com/b/dsyme/archive/2010/01/08/f-interactive-tips-and-tricks-visualizing-data-in-a-grid.aspx)
* [Joe Mayo](https://twitter.com/JoeMayo) for his awesome [LINQ to Twitter](https://linqtotwitter.codeplex.com/)



