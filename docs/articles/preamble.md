---
uid: articles.preamble
title: Preamble
---

> [!NOTE]
> The library is built for the latest DSharpPlus nightly version (v5.0).
> If you are still using DSharpPlus v4. Please read about nightlies at the
> [DSarpPlus](https://dsharpplus.github.io/DSharpPlus/index.html)

## Prerequisites

This library is not really meant to be easily hot swappable with your own implementation.
You should instead use [DSharpPlus.Commands](xref:articles.commands.introduction) for that kind of
functionality.

You don't really need to know DSharpPlus too well to use this library more than the basic setup.
But this requires a basic knowledge over the structure for a ASP.NET ApiController as building
commands
with `Instellate.Commands` works in a similar fasion.

This library also expects you to know what the Discord API wants. As this library won't
do any validationsfor you. This means if you for example define command metadata incorrectly you'll
most likely get an API error from discord rather than this library telling you what you did wrong.
