module Crawler.CrawlerTypes

open System

type CrawlDocJob = { Uri: Uri }
type CrawlJobWithImages = { Uri: Uri }

type CrawlJob = 
    | CrawlDocJob 
    | CrawlJobWithImages

type CrawlDocResult = { RootUri: Uri; Links: Uri list; ImageLinks: Uri list; Size: int }
type CrawlImageResult = { ImageUri: Uri; Size: int }
type CrawlFailedResult = { RootUri: Uri; Reason: string }

type CrawlResult = 
    | CrawlDocResult
    | CrawlImageResult
    | CrawlFailedResult