## Setup

```
PUT _bulk
{ "create" : { "_index" : "movies", "_id" : "135569" } }
{ "id": "135569", "title" : "Star Trek Beyond", "year":2016 , "genre":["Action", "Adventure", "Sci-Fi"] }
{ "create" : { "_index" : "movies", "_id" : "122886" } }
{ "id": "122886", "title" : "Star Wars: Episode VII - The Force Awakens", "year":2015 , "genre":["Action", "Adventure", "Fantasy", "Sci-Fi", "IMAX"] }
{ "create" : { "_index" : "movies", "_id" : "109487" } }
{ "id": "109487", "title" : "Interstellar", "year":2014 , "genre":["Sci-Fi", "IMAX"] }
{ "create" : { "_index" : "movies", "_id" : "58559" } }
{ "id": "58559", "title" : "Dark Knight, The", "year":2008 , "genre":["Action", "Crime", "Drama", "IMAX"] }
{ "create" : { "_index" : "movies", "_id" : "1924" } }
{ "id": "1924", "title" : "Plan 9 from Outer Space", "year":1959 , "genre":["Horror", "Sci-Fi"] }
```

```
POST /movies/_analyze
{
   "tokenizer" : "standard",
   "filter": [{"type":"edge_ngram", "min_gram": 1, "max_gram": 4}],
   "text" : "Star"
}
```

```
PUT movies_auto_complete
{
  "settings" : { "number_of_replicas": 0 }, 
   "mappings": {
       "properties": {
           "title": {
               "type": "search_as_you_type"
           },
           "genre": {
               "type": "search_as_you_type"
           }
       }
   }
}
```

```
POST _reindex
{
 "source": {
   "index": "movies"
 },
 "dest": {
   "index": "movies_auto_complete"
 }
}
```