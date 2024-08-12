# Graphics
Graphics settings plugin for AIGirl and HoneySelect2

Inspired and heavily influenced by PHIBL.

Please refer to unity's [post processing stack manual](https://docs.unity3d.com/2018.2/Documentation/Manual/PostProcessingOverview.html) for detailed description of the settings. [日本語](https://docs.unity3d.com/ja/2018.2/Manual/PostProcessingOverview.html)

Similar to PHIBL, Graphics uses cubemaps for imaged based lighting (IBL). Some sample cubemaps available [here](https://mega.nz/#F!PEMRkASB!I0ZTv4OgV-mSxX07MWDMQw). Put them in a folder named "cubemaps" at the root of the installation folder. You can configure the path for them with [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager).

# Note from OrangeSpork

While Hooh labs works on the proper overhaul of this mod, the version of HS2Graphics I've been packaging with the VR plugin has picked up a bunch of...non-VR related stuff in it. I'm finally breaking down and moving the release out of the VR plugin over into the proper repo. 

This is totally optional, if you don't know why you want this, you can just stay on what you've got, no issues.

Installation: Remove any existing Graphics.dll or .dl_, unzip the zip to game root. If you have a recent repack, InitSettings will pick up the new HS2Graphics.dll version and let you control it normally via the checkbox.

Defaults: It comes with default presets, if you don't care for the out of the box ones, just load the preset you want and/or fiddle with settings and then hit the 'Save current as default' button on the appropriate default type (Main Game/Maker/Studio/Title/VR) on the presets tab. 

Major changes list:

- Default Presets - Graphics fires up on the title screen now and auto-loads the default preset when you jump between scenes (especially handy in Main Game)
- SSS fixes and improvements (mirrors, phone cams, etc), profile per object implemented, culling layer implemented, etc.
- Fix to prevent graphics window spawning off screen
- Fix to allow VNGE and Graphics options window to both be up at the same time
- Studio toolbar icon
- Lights and Reflection Probe settings save and load along with scenes
- Pulsed Realtime Reflection Probes setting - Updates realtime reflection probes every N seconds, benefits of realtime probes with less overall performance impact.
- Save Customized Light Profiles for each Main Game Scene
- Numerous small bug fixes

# FAQ

**How do I bring up the settings?**

Press F5 at any time. In studio you can also use the toolbar icon. 

**How do I create a preset?**

Just bring up settings with F5, navigate to the presets tab, enter a name for your preset in the text field and click save. If you'd like you can also set the current setup as the default preset using the buttons down lower. It is recommended to always create a named preset for any defaults you update so you have a permanent copy of your prior versions.

**I have a preset file, where does it go? I have presets or cubemaps but they don't show in game?**

Press F1 -> Plugin Settings ->  Graphics and look at where your directories are pointing to, fix them if necessary. By default that's the presets folder in your game install for presets and cubemaps for cubemaps. You may need to create the directory if it didn't previously exist. If you move installations, this is probably pointing at the old spot, hitting reset on the setting will move it to your current install location (or you can always manually set it).

**Can I get back to vanilla settings?**

No. For one thing there is no singular vanilla settings, the game applies different settings in different scenes - so it's flat not possible to have a 'vanilla' preset. However, there are some presets available that mimic the vanilla feel, but it's never going to be an exact match in all scenarios.

**My presets don't save my light changes?**

Presets do not save light or cubemap settings, for Studio lighting is saved as part of the scene. For the main game lighting can be saved via a button on the lights tab. There is just one save slot per scene. Note that the same map is used in different scenes. For example the lobby selection screen, lobby ADV and lobby H Scene are all separate scenes and have separate light setups. Lighting presets are stored in the mapLightPresets subdirectory of the presets folder by scene name.

**Bloom doesn't seem to do anything?**

Try lowering threshold...slider operates backwards as to how folks tend to think it should. Also make sure the bloom color isn't black.

**I'm having issues with rendered (F11) screenshots...DOF doesn't work? SSS doesn't render?**

Unity DOF doesn't work with upsampling, you'll need to pick one of the two features, either turn off DOF or set the upsampling slider to 1 in the Screenshot plugin settings.

For SSS switch from Forward to Deferred rendering.

**I turned the cam light off but my scene loads with it back on?**

Known issue, game and plugin are fighting over control. Workaround is to turn the intensity down rather than turn it off. Generally speaking, if the game and graphics both expose a setting, you'll need to modify both to the same thing to ensure consistency.

**My scene looks a bit different when I load it vs. if I reapply the preset?**

There are known issues around skybox/cubemap settings not always applying correctly during scene load. Future improvements are in the works but for now try to 1) set vanilla game settings the same as graphics where possible and 2) if necessary reload the preset after the scene loads and stabilizes. In rare scenarios you may actually need to create two presets of the same settings and switch back and forth (double apply the preset) to get everything in correctly (I think I got all the issues that drove that need, but as a last resort that should always work).

**My 3090 has a bad framerate?**

See the performance tips below, but there is definitely a driver side issue with the 3090 cards. They are simply pumping out lower framerate for the *same presets* as a 20X0 card - which shouldn't ever be a thing. If anyone can identify a specific setting that relates to the fps drop (assuming there even is one) let me know and we can try to work around, but realistically this'll probably be a thing until some random driver update makes it go away just as mysteriously. 

**Sitting on the Lights tab with a light open drops my framerate significantly?**

Yep, especially with the advanced settings showing. Some of the individual light options are expensive to query the state of. So...don't linger, get in, set things up and get out.

**My cubemap/skybox isn't showing no matter what I do?**

Check the far clip plane...make sure that is at the full 15,000, most cubemaps/skyboxes show up around 14,500ish, so a closer far clip plane will cause them not to render.



# Performance Tips

My FPS is terrible, halp...

Does Graphics mod make the game slower. Short answer is no. Long answer is yes. Because 3 letters is longer than 2...thank you, thank you, try the veal.

But seriously, Graphics mod doesn't do anything itself, instead it allows you to access and enable advanced Graphics features not normally turned on in the game. Some of which come at a performance cost. If you have the same settings as Vanilla you'll get the same performance as Vanilla. Most people are using this mod specifically to improve the looks of things, but that degrades performance. It all depends on your preset. So, let's dive it to a discussion of the options and I'll point out some of the most expensive in terms of FPS and you can make your own choices.

## Starting with the Settings tab:

**Clear Flags** - Ignore these. You either know what these do and know not to touch them or you don't know what they do...and so shouldn't touch them.

**Near/Far Clipping Plane**. Basically makes stuff closer or further than the distance specified not render. Near clipping plane is mostly used to prevent stuff really close to the camera from clogging up the view. I recommend leaving this at a very small value (0.01 -> 0.1). There are better tools to help with this, see the HS2 Map Masking plugin stuff. Far clip plane allows you to save performance by not drawing stuff that's very far away. This game however really doesn't tend to actually have stuff very far away. If you can't see the skybox, check to see if this is set too close.

**Rendering Path** Hmm, complicated one. VertexLit is an option you'll ignore, it's only for some very stylized type games and stuff. Forward and Deferred are interesting choices though. The difference is how shadows and some occlusion (things in front of other things) are handled. Short version is that doing stuff right for calculating shadows gets very complicated very quickly. Sure, this object is in front of this other object and should block it and cast a shadow right? Nope, partially transparent...blegh. Hence all the excitement about hardware ray tracing to handle this. Meantime, Unity Engine handles this quandary by offering two modes. Forward mode calculates shadows and stuff more accurately. But to avoid crippling performance issues Unity only handles shadows from the 4 strongest light sources present. Deferred calculates for all lights, but uses an approximation mode that's less accurate but faster. Use Forward when you have few, strong lights (like a daytime scene with just the Sun). Use Deferred if you have lots of smaller lights. 

**Field of View** Self explanatory.

**Occlusion Culling** Unity spends extra CPU time calculating stuff it can save GPU time by not needing to render. Only works with baked in occlusion rendering (built into the map) which Illusion is notorious for not using. Can probably leave this off and save time as a result, but some modded maps may actually use this correctly. Unless your CPU bound, this won't affect FPS either way.

**Allow HDR** Allows HDR. Better color handling and allows for certain post processing effects (like post-exposure). Small performance hit on some systems (especially VR).

**Allow MSAA** Allows Multisampling AntiAliasing (see the multiplier set below). Only works in Forward rendering mode. A bit redundant to the algorithms available over in PostProcessing.  Personally I find it does basically nothing over SMAA and has a larger frame rate hit. Recommend off.

**Allow Dynamic Resoultion** Not supported on PC in this Unity version...ignore.

**Pixel Light Count** Maximum number of lights that effect an object, additional dimmest lights render as Vertex lights only (low quality). Don't touch this. 

**Anisotropic Textures** Do you have a 15 year old GPU? Turning this off will net you some FPS. For all modern GPUs they do this in their sleep. Force Enabling sets this to 8x. Enabling leaves it at what Illusion has, which may vary from scene to scene. Monkey testing seems to indicate Forced Enable results in sharper textures than the game base settings, your mileage may vary. No real FPS hit here either way.

**MSAA Multiplier** See MSAA above.

**Realtime Reflection Probes/Pulse/Pulse Timing** A reflection probe is a thing that calculates how ambient light, shadows and whatnot effect the lighting of an area of the scene. In most games you can precalculate this since your major light sources and map stuff is all fixed. This is part of 'baked' lighting. Realtime probes in comparison continuously update such effects with the latest information. Given that stuff (especially in Studio) moves around and can't be pre-baked entirely as we are basically building the scene realtime, this is rather handy for correct lighting but is, as you might imagine, expensive to calculate. Enabling realtime probes will hit your FPS noticeably. As a partial solution, the pulse feature allows you to turn the realtime probes on for a few frames every N seconds. Rather than continuously updating things it updates every few seconds instead. While studio scenes change on the fly when you load/add/move stuff, they don't move the stuff that effects this (like lights especially) continuously. So this grants the advantages of realtime probes at a much cheaper cost, if you aren't moving lights or major scene pieces rapidly around the scene. Small movements and angle changes aren't an issue. Recommendation: Pulsed Probes at about 5-10 second update interval. 2 seconds if you have a strong CPU.

**Shadowmask Mode** Simplifying things but Distance better/more expensive (better at casting map shadows onto dynamic objects in simple terms). Very small (read imperceptible) actual performance difference in practice. Recommend setting to Distance. If you want more framerate, turning shadows off entirely is better.

**Shadows** Let's you turn shadows Off, Hard mode means sharp edged (but cheaper) shadows. Soft is more realistically shaded (but expensive and tend to have alias artifacts). Switch to hard or even off to get more FPS.

**Shadow Resolution** Higher = more shadow quality, Lower = better performance. Much smaller impact then turning shadows completely off, but might be a good compromise.

**Shadow Projection/Distance/Near Plane** These affect visual look and not really performance. Generally scene specific, recommend leave these alone and then adjust only if you want to fine tune the look of shadows in a particular scene.

**Language/Font/WindowSize/Advanced Mode** Graphics mod options for itself...no performance impact here..duh.

**Advanced Mode Stuff**

**Culling Mask** - Don't touch.

**PCSS** - See here: https://github.com/TheMasonX/UnityPCSS - Basically fancy shadows...bit fiddly and scene specific.

## Presets Tab

Lists all your presets from your presets folder (default is...presets in the game directory). Use the F1 plugin settings menu to check where this is pointed at. Note: it is an absolute path, so if you move the game, it's going to be still pointed to the last place.

There's a save box, type in a name and click save to save the current settings as the new preset file name. You can share these around. 

Finally the defaults. These are what gets loaded when scenes (in the Unity sense) change in each of the listed modes. Note that for studio only the INITAL load into studio counts as a 'Scene' change. Loading Studio scenes doesn't count as a Unity scene change. Main game changes scenes every time you sneeze. Load immediately loads that preset. Save updates the preset to your current settings. Reset sets it back to the out of box configuration.

## SSS

Used for Subsurface Scattering Effects...which is currently...one, Hanmens Nextgen Skin Mod. Though presumably more will come in time. So if you aren't using Hanmen Nextgen Skin mod...this does nothing, turn it off. If you are using Hanmen Nextgen skin...turn this on, duh.

**Profile Per Object** Determines if the subsurface scattering color is determined by the object being looked at or the setting here. Turning this off lets you set the color. Leave this on.

**Blur Size** Look and feel trade off. Increasing this makes the scattering effect 'better' but will blur fine skin features. Set to taste, no real performance impact.

**Postprocess Iterations** More is higher quality at higher cost. Strong diminishing returns effect, around 2-4 is plenty.

**Shader iterations per pass** Ditto. I find higher than about 6 does essentially nothing I can see.

**Downscale factor** Divides the resolution of the object when calculating. BIG performance gain but lowers fidelity and can introduce aliasing artifacts. For beauty shots leave it at 1 (full size). For moving scenes (like in the main game) I find setting this to 2 provides large frame rate improvements (almost refunds SSS being on entirely) at minimal quality loss.

**Max Distance** - Not implemented.

**Debug Distance** - Not implemented.

**Layers** What the SSS checks and processes. Only enable Chara and/or Map basically. Use Chara for skins and Map for SSS studio accessories primarily. Since we don't have any SSS studio accessories setting this to Chara being the only selected layer will grant some fps back, but won't process some map obstructions changing lighting on the character skin. Normally this isn't particularly noticeable unless you have a really shadow heavy scene with map elements in the field of view, and even then it isn't enormous. Chara only is good enough for most scenes, you can enable Map before key screenshots for the literal extra bit of fidelity if you like. Other layers are noramlly irrelevant.

**Dither** Turns dithering on/off. Dithering blurs the underlying skin features a bit and looks more 'natural'. CPU performance hit, unless you are CPU bound this doesn't cost you anything. If you are CPU limited, turning this off will grant some FPS but make the underlying skin a bit...blotchy.

**Intensity** More is a stronger effect and more expensive. I find I can't tell much difference above 2 (or things just get blurry).

**Scale** ...leave this at default basically.

**Debug** Umm, debug stuff for me basically...

## SSS Performance Tips

Most important: Turn off all layers except Chara.\
Next: Downscale factor. Turn this up to 2.0 for big frame rate gains, quality loss...isn't bad honestly. Try for even numbers (it's a divider) (1, 2, 4).\
Next: Postprocess iterations/Shader Iterations (lower is faster)\
Dither/Dither Intensity: If you are CPU bound this'll grant performance, otherwise doesn't help.

I find Chara only layer, Downscale of 2, 2 postprocess iterations, 4 shader iterations, dither on...looks quite nice and the framerate hit is surprisingly small.

## Post Processing

**Anti-aliasing** Makes the Jaggies go away. In order of both performance impact and quality it goes None->FXAA->SMAA->TAA. Unless your graphics card is ancient however, FXAA or SMAA won't even be noticeable, so turn on one of these and SMAA is frankly just straight better. If you have a really Potato PC turn the quality down a bit to Medium even Low if you need to, but honestly doubt you'll notice it on.

**TAA** - TAA needs a discussion all by itself. It's a bit more expensive (barely) than SMAA. It CAN produce much better results than SMAA, the problem is that one size does not fit all for this mode. Basically it makes things look better and better until it goes too far and makes things flash (especially Pantyhose, gloves and other skin tight stuff). Some studio items as well get blinky with this turned up too far. 

**Sharpness** - Higher means more processing, stronger de-aliasing but if you crank it too far everything will get artificially straight/blocky.

**Stationary/Motion Blending** Controls how much history goes into the TAA algorithm for moving/non-moving items. In simple terms...Lower equals stronger de-aliasing, higher does a better job avoiding...flicker in simple terms. Basically lower until you get flicker and then increase.

**Jitter Spread** TAA...subtly moves stuff and then uses that to try and prevent unrealistic anti-aliasing issues. Too high however will cause flicker. Basically you want the highest value you can hit without stuff flickering.

Basically TAA can look the best of all of them, especially with some fine detail stuff like hair (at least as good as HS2 gets), but is finicky. I recommend using SMAA most of the time and then doing a per-scene TAA, setting Jitter as high and blending as low as possible until you get flicker. TAA can also cause ghosting sometimes, especially if you are having low FPS issues, if you get that just go back to SMAA. Note, in VR mode TAA hits the UX and will make it blurry, either use SMAA or...suffer with a blurred UX.

**Post Process Effects**

None of these are really particularly expensive and I haven't noticed any real performance issues with them...Mostly they effect how things actually look. What they all do is waaayyy too long for here, just RTFM: https://docs.unity3d.com/Packages/com.unity.postprocessing@2.1/manual/Ambient-Occlusion.html

## Lights

Individual light settings 

**Alloy Light** - Advanced Setting - Slightly better light look, small performance hit. Leave it on.

**Light Controls** - You can add lights here (same as adding in Studio) and turn lights on/off. Turning lights off removes lighting and shadow work so makes things faster. Performance impact of any particular light depends on numerous factors such as what all it affects, the shaders being used, other graphics settings. So hard to say. Fewer is faster in general, but most lights have a small FPS effect, usually requires large numbers of lights to really make a difference. Note that directional lights, since they hit everything are most expensive, Points are generally cheaper, spots...vary.

**Color** - Color of the light...duh. No performance impact, just...visual.

**Shadows** Same as shadow discussion above, really. Note, bias/normal bias and near plane all refer to the shadows cast by the light. No real performance issues here but do change the way it looks, sometimes dramatically, fun to play with.

**Intensity** - Higher=stronger light. Note, certain effects (especially in Forward mode) only apply to the 4 or so strongest lights.

**Indirect Multiplier** In theory this effects how much the light bounces to hit objects it doesn't directly see. Very scene specific setting.

**Rotation** - Another way to set the direction of the light (same as the studio controls)

**Specular Highlight** - Strength of the Alloy lighting effect...leave at default.

**SEGI Sun Source** - Use this if you know what SEGI does, otherwise pretend you don't see this.

**Range** - For spots and point lights, how far they reach

**Angle** - For spots, same as the studio control - angle of the spotlight cone.

**Render Mode** - Advanced Setting - Interesting choices. ForcePixel is higher quality but more expensive, ForceVertex is cheaper but faster. Auto lets the game pick (I believe it always picks Pixel). If you just want a cheap, background sorta filler light to supplement better lighting, setting ForceVertex is an interesting option, but normally stick with Pixel.

**Culling Mask** - What the light effects. You can make the light hit only certain things by changing this. Can be handy in some modded maps that may have stuff in strange layers.

## Lighting

This tab deals with scene wide effects such as skyboxes and reflection probes.

**Environment Skybox** -- Somewhat misnamed, allows you to pick a cubemap. A cubemap is a skybox (an enormous box the scene is basically inside of) that also provides lighting emanating from it into the scene - so both a texture in the sky and a source of light. Cubemaps are from the cubemaps folder (check F1 plugin settings for your location). 


**Source** - Ambient scene lighting - one of four modes Custom (not shown but the mode if you picked a cubemap), Skybox (either from the selected skybox (NOT CUBEMAP) selected from the Illusion scene options if you picked one or the single color picked below), Flat (Single color from Illusion scene options - Not sure this works at all honestly), Trilight (3 colors Sky/Ground/Horizon) from the Illusion scene options. 

**Intensity** - With a cubemap or skybox selected, determines the brightness of ambient light produced by the skybox/cubemap.

**Exposure** - Brightness of the cubemap/skybox itself - does nothing in other modes.

**Rotation** - Rotation of the cubemap/skybox

**Tint** - Color tint applied to the cubemap/skybox

**Resolution** - Skybox only - Resolution of the skybox for purposes of reflections (more is higher quality, but more expensive). Only for skyboxes. More = Quality, Less = Faster

**Intensity** - How strongly the reflected cubemap/skybox appears on items reflecting it. 

**Bounces** - Maximum number of times light is allowed to bounce off objects. 1 means the lights reflect off one object, higher than 1 means reflections can chain. Big performance impact here potentially dependingon lighting source and number of reflecting objects.

**Reflection Probes** - Long and complex, short version? RTFM: https://docs.unity3d.com/2018.4/Documentation/Manual/ReflectionProbes.html

For performance purposes, Lower resolution, turn off HDR and set your Culling Mask and Time Slicing Mode correctly to reduce performance hit. However, see the Pulsed Realtime setting to get a possible High Quality/Fast Performance compromise, assuming you don't have lights and objects zooming around the scene rapidly.

## Attributions
[Alloy](https://github.com/Josh015/Alloy)  
[SEGI](https://github.com/sonicether/SEGI)  
[PCSS](https://github.com/TheMasonX/UnityPCSS)  
[keijiro/PostProcessingUtilities](https://github.com/keijiro/PostProcessingUtilities)  
[IllusionModdingAPI](https://github.com/IllusionMods/IllusionModdingAPI)
