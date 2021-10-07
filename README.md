# RSI Diff Bot

A github action for generating diffs of RSI (robust station images), a format used by Robust Toolbox. It runs on every pull-request push, and comments on the PR
with a nicely formatted table of any changes. The action will not make more than one comment per PR, and it does not run on direct commits nor on PRs that have not
changed any RSIs.

![example](https://i.imgur.com/ZJcBaX2.png)

An example workflow is provided in `.github/workflows/example.yml`. You'll have to tweak it obviously, but that provides everything you need to get started.
