# LunarVR stereo lens-flare correction

This embedded package is Unity URP 17.3.0 from Unity 6000.3.8f1.
It preserves the installed package version and its dependencies.

## Changes

In the single-pass XR color loop of both `PostProcessPassRenderGraph.cs`
and `PostProcessPass.cs`, pass `xrIdx` to
`LensFlareCommonSRP.DoLensFlareDataDrivenCommon` instead of `xr.multipassId`.
The core renderer uses that argument as the destination texture-array slice.
The original loop sends both projections to slice zero in single-pass XR,
leaving the right eye without a flare. Mono and multipass branches are unchanged.

`ShaderLibrary/Shadows.hlsl` uses package-qualified includes for `Core.hlsl` and
`Shadows.deprecated.hlsl`. The relative includes failed shader compilation after
embedding on this Windows editor; this changes no shader behavior.

## Maintenance

Keep this package with the project; editing Library/PackageCache would not persist.
When upgrading Unity/URP, check whether the upstream single-pass loops use the
per-eye index before removing this embedded copy. Verify both eyes on Quest Link
and an Android build, including terrain occlusion and head rotation.

Scene-side settings in Assets/LunarVR.unity enable post-processing and depth on
the eye cameras. The native flare is attached to SunDisc, with the directional
light as its light override and constant distance/scale curves. This co-locates
the image and flare and tests occlusion at the actual sun-disc distance instead
of the camera's far clipping plane. The old directional flare and SunFlareQuad
fallback are disabled but retained. A transparent, non-depth-writing material
prevents the sun image from occluding its own flare.

The editor menu Tools/Sun switches between this native setup and the retained
image fallback without deleting either.
