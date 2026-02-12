<a name="readme-top"></a>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/K4ryuu/K4-BombWires-SwiftlyS2?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/K4ryuu/K4-BombWires-SwiftlyS2?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/K4ryuu/K4-BombWires-SwiftlyS2?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/K4ryuu/K4-BombWires-SwiftlyS2?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/K4ryuu/K4-BombWires-SwiftlyS2/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://dsc.gg/k4-fanbase)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">KitsuneLab©</h1>
  <h3 align="center">K4-BombWires</h3>
  <a align="center">A bomb defuse minigame for Counter-Strike 2 using <del>SwiftlyS2</del> CounterStrikeSharp framework. Terrorists select a wire color when planting, and CTs must guess correctly to instant defuse or the bomb explodes!</a>

  <p align="center">
    <br />
    <a href="https://github.com/K4ryuu/K4-BombWires-SwiftlyS2/releases/latest">Download</a>
    ·
    <a href="https://github.com/K4ryuu/K4-BombWires-SwiftlyS2/issues/new?assignees=K4ryuu&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D">Report Bug</a>
    ·
    <a href="https://github.com/K4ryuu/K4-BombWires-SwiftlyS2/issues/new?assignees=K4ryuu&labels=enhancement&projects=&template=feature_request.md&title=%5BREQ%5D">Request Feature</a>
  </p>
</div>

### Support My Work

I create free, open-source projects for the community. While not required, donations help me dedicate more time to development and support. Thank you!

<p align="center">
  <a href="https://paypal.me/k4ryuu"><img src="https://img.shields.io/badge/PayPal-00457C?style=for-the-badge&logo=paypal&logoColor=white" /></a>
  <a href="https://revolut.me/k4ryuu"><img src="https://img.shields.io/badge/Revolut-0075EB?style=for-the-badge&logo=revolut&logoColor=white" /></a>
</p>

### Dependencies

To use this server addon, you'll need the following dependencies installed:

- ~~[**SwiftlyS2**](https://github.com/swiftly-solution/swiftlys2): SwiftlyS2 is a server plugin framework for Counter-Strike 2~~
- [**CounterStrikeSharp**](https://github.com/roflmuffin/CounterStrikeSharp/releases): CounterStrikeSharp allows you to write server plugins in C# for Counter-Strike 2/Source2/CS2.
- [**CS2MenuManager**](https://github.com/schwarper/CS2MenuManager)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- INSTALLATION -->

## Installation

1. Install [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases) on your server
2. Install [CS2MenuManager](https://github.com/schwarper/CS2MenuManager/releases) on your server
3. [Download the latest release](https://github.com/rogeraabbccdd/K4-BombWires/releases/latest)
4. Extract to your server's `counterstrikesharp/plugins/` directory
5. Configure `counterstrikesharp/configs/plugins/K4-BombWires/K4-BombWires.json` (optional)
6. Edit `counterstrikesharp/shared/CS2MenuManager/config.toml`, set `ForceConfigSettings = false` (optional)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- FEATURES -->

## Features

- **Wire Selection Minigame**: Terrorists choose a wire color when planting the bomb
- **Instant Defuse**: CTs who guess the correct wire instantly defuse the bomb
- **Instant Explosion**: CTs who guess wrong cause the bomb to explode immediately
- **Random Fallback**: If T doesn't select a wire, one is chosen randomly
- **Menu Timeout**: Configurable timeout for wire selection menu
- **Multi-language Support**: Full localization support

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- GAMEPLAY -->

## How It Works

1. **Terrorist plants bomb** → Wire selection menu appears
2. **T selects a wire color** (Red, Blue, Yellow, or Green) or ignores for random
3. **Bomb is planted** with the selected/random wire
4. **CT begins defusing** → Wire selection menu appears
5. **CT guesses a wire color**:
   - ✅ **Correct**: Bomb instantly defused!
   - ❌ **Wrong**: Bomb instantly explodes!

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONFIGURATION -->

## Configuration

### config.json

```json
{
  "K4BombWires": {
    "MenuTimeout": 5,
    "RandomizeWireOrder": false
  }
}
```

| Option        | Description                          | Default |
| ------------- | ------------------------------------ | ------- |
| `MenuTimeout` | Seconds before wire menu auto-closes | `5`     |
| `RandomizeWireOrder` | If enabled, the order of wire colors in the menu will be randomized. | `false`     |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->

## License

Distributed under the GPL-3.0 License. See [`LICENSE.md`](LICENSE.md) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
