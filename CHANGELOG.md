## 0.1.19 (27-12-2021):

Use `ToString` method only when `Parse`/`TryParse` is available.

## 0.1.18 (06-12-2021):

Added `net6.0` target.

## 0.1.16 (13.09.2021):

- Fixed bug with rooted paths.
- FileWatcher is now able to handle cases when parent folder is deleted while being observed.

## 0.1.15 (09.06.2021):

Fixed bug with Rx and debugger for all sources.

## 0.1.14 (09.06.2021):

Fixed bug with Rx and debugger.

## 0.1.13 (03.07.2020):

FileSource now resolves relative file paths from AppDomain.CurrentDomain.BaseDirectory.

## 0.1.12 (10.05.2020):

Implemented SwitchingSource that allows to dynamically switch the underlying source without invalidating current subscriptions.

## 0.1.11 (05.05.2020):

TemplatingSource no longer fails on null substitution values.

## 0.1.10 (04.05.2020):

- Added FrozenSource and Freeze() extension to turn dynamic sources into static ones.
- Added TemplatingSource and Substitute() extension to substitute placeholders in nodes values.

## 0.1.9 (08.02.2020):

Added NodeTransformer and ValueNodeTransfomer � helpers for TransformingSource.

## 0.1.8 (01.02.2020):

Implemented NestingSource.

## 0.1.7 (11.01.2020):

CommandLineSource: support for single-hyphen key syntax, default keys and values.

## 0.1.6 (10.12.2019):

Implemented TransformingSource.

## 0.1.5 (23-09-2019):

Fixed ObjectSource.
Implemented ObjectSourceSettings.

## 0.1.4 (13-09-2019):

Implemented ObjectSource.

## 0.1.3 (15-03-2019):

Fixed possible assembly resolution issues caused by this library.

## 0.1.2 (03-03-2019): 

Fixed the problem with ILRepack + Rx + debugger.
FileSource now reacts to file creation/rename events immediately.

## 0.1.1 (19-02-2019): 

Implemented CommandLineSource.

## 0.1.0 (18-02-2019): 

Initial prerelease.