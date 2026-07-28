docker exec crafty-db pg_dump ^
  -U prime8bit ^
  --inserts ^
  --column-inserts ^
  crafty > DbBackup.sql

REM Unfortunately, you will need to manually remove inserts to Crafts.SearchImageId to prevent circular PK problems. You also need to delete inserts to EFMigrationsHistory because they are reinserted by EF Core.