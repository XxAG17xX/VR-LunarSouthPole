<a href="https://youtu.be/z2SDj-kG6RA"><img src="docs/media/hero.jpg" alt="VR Lunar South Pole: the Eagle lander on the lunar surface with Earth above the horizon. Click to watch the walkthrough." width="100%"></a>

<p align="center">
  <img src="https://img.shields.io/badge/Unity-6-222222?style=flat-square&logo=unity&logoColor=white" alt="Unity 6">
  <img src="https://img.shields.io/badge/Meta_Quest_3-standalone-2d4f86?style=flat-square&logo=meta&logoColor=white" alt="Meta Quest 3, standalone">
  <img src="https://img.shields.io/badge/URP-C%23_%C2%B7_HLSL-3b3b52?style=flat-square" alt="URP, C#, HLSL">
  <img src="https://img.shields.io/badge/terrain-NASA_LRO_%2F_LOLA-1f4d63?style=flat-square&logo=nasa&logoColor=white" alt="Terrain from NASA LRO / LOLA">
  <img src="https://img.shields.io/badge/final--year_project-April_2026-4a4a4a?style=flat-square" alt="Final-year project, April 2026">
</p>

<p align="center">
  <b>Kartik Gupta</b> · B.A.I. Computer Engineering · Trinity College Dublin<br>
  <a href="https://youtu.be/z2SDj-kG6RA">Watch the walkthrough</a> ·
  <a href="#the-mission">The mission</a> ·
  <a href="#the-research">The research</a> ·
  <a href="#how-it-works">How it works</a> ·
  <a href="#explore-the-source">Explore the source</a>
</p>

<table align="center">
  <tr>
    <td align="center"><h3>1.625 m/s²</h3><sub>lunar gravity</sub></td>
    <td align="center"><h3>1.5°</h3><sub>sun above the horizon</sub></td>
    <td align="center"><h3>1 km²</h3><sub>of real NASA terrain</sub></td>
    <td align="center"><h3>~900 m</h3><sub>EVA traverse</sub></td>
    <td align="center"><h3>~71 FPS</h3><sub>on a standalone Quest 3</sub></td>
  </tr>
</table>

## What is this?

Picture stepping off a lunar lander, grabbing a science instrument, and carrying it across terrain you've never seen, into shadow, with the Earth hanging just above the horizon. That's what this project turns into a standalone VR experience.

I rebuilt a small patch of the lunar south pole from NASA elevation data, swapped Unity's Earth-like defaults for documented lunar parameters, and gave the player a full instrument-deployment mission to carry out.

The question driving it wasn't just *can the Moon look convincing in VR?* It was **how much of a scientifically grounded lunar environment can a consumer headset actually handle, and where does fidelity have to give way to practical limits?**

<p align="center">
  <img src="docs/media/lamp-route-walk.webp" alt="A few seconds of the traverse: walking through shadow along the cyan lamp chain, with a sunlit ridge and Earth above." width="440"><br>
  <sub>A few seconds from the walkthrough: following the lamp chain through shadow.</sub>
</p>

| At a glance | |
| :--- | :--- |
| **Experience** | Single-player lunar EVA (extravehicular activity) mission |
| **Where** | Haworth-Nobile region, lunar south pole |
| **Terrain** | 1,024 m × 1,024 m patch built from LOLA elevation data |
| **Runs on** | Meta Quest 3, as a standalone Android app |
| **Built with** | Unity 6, URP, C#, HLSL, Meta XR SDK |
| **Context** | B.A.I. Computer Engineering final-year project, April 2026 |

> [!TIP]
> **You don't need to download anything.** The [video walkthrough](https://youtu.be/z2SDj-kG6RA), the images here and the research summary below are the best way in. This repo keeps the full Unity project and its history, but it isn't packaged as a one-click release.

## The mission

It's inspired by lunar seismic science and the Artemis Lunar Environment Monitoring Station (LEMS) objective: a simplified interactive scenario rather than a copy of any approved mission procedure. Four steps, roughly a **900 m traverse** end to end:

<table>
  <tr>
    <td width="25%" align="center"><img src="docs/media/step-retrieve.jpg" alt="The seismometer glowing in the lander's equipment bay, ready to pick up." width="100%"></td>
    <td width="25%" align="center"><img src="docs/media/lunar-craters.jpg" alt="Cratered lunar terrain with bright ridges and deep shadows, looking back toward the distant lander." width="100%"></td>
    <td width="25%" align="center"><img src="docs/media/lamp-route.jpg" alt="A cyan-lit navigation route through shadow toward a brightly illuminated lunar ridge." width="100%"></td>
    <td width="25%" align="center"><img src="docs/media/step-deploy.jpg" alt="The DEPLOY panel on the visor HUD in the target zone, with the deployment area highlighted." width="100%"></td>
  </tr>
  <tr>
    <td valign="top"><b>1 · Retrieve</b><br><sub>Open the lander's equipment bay and pick up the seismometer.</sub></td>
    <td valign="top"><b>2 · Traverse</b><br><sub>Carry it toward the science zone, with terrain-following movement in lunar gravity.</sub></td>
    <td valign="top"><b>3 · Navigate</b><br><sub>Switch on a chain of lamps laid along the terrain and follow the visor HUD through the shadows.</sub></td>
    <td valign="top"><b>4 · Deploy</b><br><sub>Reach the target zone and hold to deploy.</sub></td>
  </tr>
</table>

Along the way, doors, proximity highlights, a navigation arrow, a deployment indicator and a quick-action menu keep things readable, so the environment rendering is tied to an actual task instead of an empty scene.

<sub>All captures are original, unretouched project images from April 2026 (the Retrieve and Deploy frames are taken from the walkthrough video). They show that version of the simulation, not every later change in this repo.</sub>

## The research

**VR Simulation of the Lunar South Pole: A Physically Parameterised Environment**<br>
Kartik Gupta · Trinity College Dublin · April 2026 · Supervisor: **Mads Haahr**

The full dissertation is on my LinkedIn, under the **B.A.I. Computer Engineering entry in the Education section**. I've kept the PDF out of this repo on purpose.

It builds on the direction of Nilsson et al.'s CHI 2023 study, *Using Virtual Reality to Shape Humanity's Return to the Moon: Key Takeaways from a Design Study*. Rather than porting their system, I built a new Unity implementation aimed at consumer standalone hardware. Tommy Nilsson also gave early guidance on the project scope, and provided the Apollo lander and Orion interior models.

### Grounded in data

Every key design parameter in the dissertation traces back to a published source:

| Parameter | What the simulation uses |
| :--- | :--- |
| Surface gravity | 1.625 m/s² |
| Sun angle | Low-angle light at 1.5° solar elevation |
| Terrain relief | NASA LRO / LOLA elevation data, 5 m/pixel source product |
| Terrain in Unity | 1,025 × 1,025 heightmap over a 1,024 m square |
| Sun and Earth size in the sky | 0.53° and 1.9° apparent size (dissertation targets) |
| Lighting | Hard direct shadows, zero ambient light |

<p align="center">
  <img src="docs/media/earth-terminator.png" alt="Earth, half in shadow, seen from the lunar surface in the simulation." width="620"><br>
  <sub>The Earth seen from the lunar surface, day/night terminator and all. Dissertation Figure 4.6.</sub>
</p>

### What the evaluation found

The evaluation combines a **fidelity-versus-feasibility analysis**, a comparison with prior work, and on-device performance measurements. On a standalone Quest 3 build, Table 5.2 of the dissertation reports about **71 FPS against a 72 Hz target, with 96% GPU utilisation**.

So it works on the headset, but with very little GPU headroom to spare.

> [!NOTE]
> **Being upfront about the limits.**
> - The parameters above are the **dissertation's design targets**, not a claim that every visual effect or later scene revision is an exact physical model. The solar cycle is deliberately sped up, Earth libration is approximated, and the starfield is a visual backdrop rather than an ephemeris-driven sky. Movement, interaction ranges and mission pacing also bend a little for usability.
> - The performance numbers are **historical measurements of the evaluated build**, not a guarantee for the current branch or every viewing direction.
> - A planned miniPXI user study couldn't run within the university's ethics-approval timeline, so this project **doesn't claim** validated training effectiveness, measured learning gains, or user-study evidence of presence.
> - It's an independent academic prototype, **not a NASA or ESA training product**. The Eagle is a historical Apollo asset used in an Artemis-inspired scenario, not a model of the planned Artemis lander.

## How it works

### From lunar data to a playable environment

```mermaid
flowchart TD
    A["NASA LRO / LOLA elevation data"] --> B["Select Haworth-Nobile region in LROC QuickMap"]
    B --> C["QGIS / GDAL: process GeoTIFF and export 16-bit heightmap"]
    C --> D["Blender: check terrain relief"]
    D --> E["Unity Terrain: build the navigable surface"]
    F["Published lunar parameters"] --> G["C# environment systems + Earth HLSL shader"]
    E --> H["Unity 6 / URP lunar scene"]
    G --> H
    H --> I["Meta XR tracking, interaction and locomotion"]
    I --> J["Retrieve, traverse, navigate and deploy"]
    J --> K["Standalone Quest 3 evaluation"]
    K -. "Fidelity and performance trade-offs" .-> H
```

The terrain pipeline and the physical parameters set up the world. Custom runtime systems then tie the Sun, Earth, player, instrument, navigation and HUD together into one mission.

### The mission state machine

```mermaid
stateDiagram-v2
    [*] --> PickUp
    PickUp --> CarryToZone: Pick up seismometer
    CarryToZone --> PickUp: Drop instrument
    CarryToZone --> Deploy: Enter deployment range
    Deploy --> CarryToZone: Leave deployment range
    Deploy --> Complete: Hold deploy to completion
    Complete --> [*]
```

These states and transitions come straight from [`MissionManager.cs`](Assets/Scripts/MissionManager.cs).

## Explore the source

The main scene is [`Assets/LunarVR.unity`](Assets/LunarVR.unity), and the code that makes it tick lives in [`Assets/Scripts/`](Assets/Scripts/).

<details>
<summary><strong>Where each system lives</strong></summary>
<br>

| System | Implementation |
| :--- | :--- |
| Mission state and instrument handling | [`MissionManager.cs`](Assets/Scripts/MissionManager.cs) |
| Surface movement, lunar gravity and crouching | [`LunarLocomotion.cs`](Assets/Scripts/LunarLocomotion.cs) |
| Solar cycle and sky synchronisation | [`LunarSunRotation.cs`](Assets/Scripts/LunarSunRotation.cs) |
| Earth motion and illumination | [`EarthBehaviour.cs`](Assets/Scripts/EarthBehaviour.cs), [`EarthDayNight.shader`](Assets/Earth/EarthDayNight.shader) |
| Lamp-chain navigation | [`LampChainNavigator.cs`](Assets/Scripts/LampChainNavigator.cs) |
| Visor UI and objective guidance | [`HelmetHUD.cs`](Assets/Scripts/HelmetHUD.cs), [`NavigationArrow.cs`](Assets/Scripts/NavigationArrow.cs) |
| Equipment-bay interaction | [`DoorController.cs`](Assets/Scripts/DoorController.cs), [`DoorHandleInteractable.cs`](Assets/Scripts/DoorHandleInteractable.cs) |
| Repeatable scene and rendering setup | [`Assets/Scripts/Editor/`](Assets/Scripts/Editor/) |

The checked-in editor version is **Unity 6000.3.8f1**; package versions are in [`Packages/manifest.json`](Packages/manifest.json) and [`Packages/packages-lock.json`](Packages/packages-lock.json).

</details>

<details>
<summary><strong>Repo storage, Git LFS, and building it yourself</strong></summary>
<br>

This repo is both a showcase and my personal development archive. The big Unity assets are kept so the work can be revisited, not because anyone's expected to download and build it.

[`.gitattributes`](.gitattributes) sends large asset types (textures, models, audio, Unity `.asset` files) through **Git Large File Storage (LFS)**. Git itself only stores small pointer files, and the actual binaries live separately in LFS, so a checkout without them isn't a complete Unity project. More in [GitHub's Git LFS docs](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-git-large-file-storage). The small images under `docs/media/` are deliberately kept in regular Git, so this README doesn't need any LFS downloads.

If you do want to open it locally: use the recorded Unity version, pull the LFS objects, and let Package Manager resolve the dependencies. Testing on a device also needs Android build support, the relevant Meta/OpenXR setup and an authorised headset connection. I can't promise a clean-machine build from this README alone.

There's also an embedded URP package with local stereo-flare changes; its [patch notes](Packages/com.unity.render-pipelines.universal/LUNARVR-PATCH.md) explain why it's kept. Rendering and headset tweaks made after the dissertation aren't the configuration evaluated in the report, and flare alignment and occlusion still need device-specific testing.

Appendix B of the dissertation mentions an earlier repository URL and shader location. **This repo is the current project archive**, and the links above match its actual layout.

</details>

## What's next

The prototype is deliberately small: one terrain region, one player, one EVA. Terrain texturing is simplified, light bouncing off neighbouring terrain isn't modelled, celestial motion is approximate, and spacesuit biomechanics are out of scope. The Orion interior was explored during development but isn't part of the finished surface mission.

The dissertation points to where it could go from here: **a proper user evaluation, better terrain shading, ephemeris-driven sky motion, astronaut embodiment, a lunar rover, and bigger multi-scene environments.**

## Thanks and references

- **Mads Haahr**, my project supervisor at Trinity College Dublin.
- **Tommy Nilsson**, for early scoping guidance and for the Apollo lander and Orion interior models, as acknowledged in the dissertation.
- **Nilsson et al., CHI 2023:** [Using Virtual Reality to Shape Humanity's Return to the Moon](https://doi.org/10.1145/3544548.3580718), the main prior-work reference.
- **NASA LRO / LOLA and LROC QuickMap:** the terrain data everything is built on. The full acquisition and processing method is in dissertation Sections 3.2 and 4.1.
- **NASA Scientific Visualization Studio:** [Earth and Sun from the Moon's South Pole](https://svs.gsfc.nasa.gov/4944/), a celestial-geometry reference used in the research.

Third-party models, textures, audio and packages keep their own terms; having them in this archive doesn't grant permission to reuse or redistribute them. Where each README image came from is recorded in [`docs/media/README.md`](docs/media/README.md).

---

<p align="center"><b>Best place to start:</b> <a href="https://youtu.be/z2SDj-kG6RA">watch the lunar EVA walkthrough</a> ▶</p>
