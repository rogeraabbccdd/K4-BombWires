# Changelog

## [1.1.0]

### Changed

- Wire menu is now a display-only panorama panel (workshop addon); W / A / S / D selects a wire, Jump confirms
- CS2MenuManager is no longer needed; requires CounterStrikeSharp 1.0.374+ (.NET 10)

### Added

- Correct wire color is shown in chat at round end
- `PanoramaLayout` config option

### Fixed

- Menu closing early when it was reopened before the previous timeout ended

## [1.0.1]

### Added

- Possibility to randomize wire menu order via config (#2)

### Fixed

- Threading issue with SendMessage - using the SwiftlyS2 Async helper (#1)

## [1.0.0] - Initial Release
