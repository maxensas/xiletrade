# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.5.1 - 2021-06-11
### Added
- Xiletrade now uses all rate limit rules dynamically when receiving responses from GGG APIs : `search, fetch and exchange`
- A `PoEDB` button has been added directly on search window and allow redirect to the list of item mods page (like `Wiki` button).

### Changed
- The UI as received some tweaks and now displays rate limit when reached, automatically pauses and timer associated.

### Fixed
- Error #13 fixed : due to bad stash name as well.

[Release notes for 1.5.1](https://github.com/maxensas/xiletrade/releases/tag/1.5.1)

## 1.5.0 - 2021-06-05
### Added
- Xiletrade display now (using tooltips), : Mod tags,  mod tiers, mod quality, range values, visual indicators for high tiers.
- Mod parsing updated for all languages (mana reserved -> reservation of skills)
- New divination cards are now set by tiers (BULK).
- Chronicle of Atzoatl is now handled, Behaviour :
Filling out the form with minimum area level, Only Tier 3 Rooms & Apex of Atzoatl. 
Auto select minimum area level and rooms : Apex of Atzoatl (boss room), Doryani's Institute (double corruption for gems), Apex of Ascension (uniques sacrifice), Locus of Corruption (double corruption for items).
- Inscribed Ultimatum is now handled :
All rewards correctly managed  : Uniques output / Currency & Div cards input / Mirrored Rare copy.
- Added management for all new currencies & Sacred Blossom.
- 2 Buttons on main window to switch between min tier values and current values (double click on real for full reset).
- Option in settings window to automaticaly choose min tier values (only Non-uniques) : This is a noticeable upgrade for Xiletrade, this option will be activated by default.

### Changed
- To recover full item infos, Xiletrade use now (internally) CTRL+ALT+C instead of CTRL-C like previously. A little flicker will be noticeable before each price-check (auto press ALT key)
- All filters have been updated : new gems, bases, uniques are now correctly handled.
- Updated poe-ninja pricing addon to follow changes made by Rasmuskl : div-cards/clusters.
- Updated resources (all languages).

### Removed
- Sliders on main window allowing changes on min/max values with a percentage.
- Search percentage option on settings window.

### Fixed
- Bugfixed : Main window can no longer be resized outside his min/maximize feature.
- Error #12 & #13 Fixed : Fetching data error (Timeout issue).

[Release notes for 1.5.0](https://github.com/maxensas/xiletrade/releases/tag/1.5.0)
