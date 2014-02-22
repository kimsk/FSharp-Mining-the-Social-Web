module Trends

open LinqToTwitter
open TwitterContext

let getTrends trend_id = 
    query { 
        for trend in ctx.Trends do
            where (trend.Type = TrendType.Place)
            where (trend.WoeID = trend_id)
            select trend
    }