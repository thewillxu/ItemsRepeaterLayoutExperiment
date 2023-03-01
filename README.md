# ItemsRepeaterShiftedLayoutExample
Reproduce a bug where the ItemsRepeater does not respect resetting the LayoutOrigin to 0,0

## Why
This projected started out as a "simple" demostration of an issue with the ItemsRepeater.  It has turned into a full fledged demo of a virtual layout with variable sized elements in C#.

## The problem
When scrolling to arbitrary positions in the layout, the context.RealizationRect passed into the MeasureOverride does not reflect changes to the LayoutOrigin.  This can cause the layout to measure and arrange items to a viewport that doesn't align to the view.

## How does it work

The demo produces a list of items of random heights and colours.

![image](https://user-images.githubusercontent.com/11729648/222229737-74dd2d43-30a7-4cb1-99bb-3d19f6360fe6.png)

The MeasureOverride process is
* Pick one item and position as an anchor
* Layout the items above and below that anchor to fill the viewport specified by context.RealizationRect
* Calculate an approximate error between the layout position and estimated position of the top element.
* Set the context.LayoutOrigin to adjust for that error.

## The bug

* Click "Scroll To Bottom"
* Scroll up with the mouse wheel for about 50 items to accumulate error in context.LayoutOrigin
* Click "Scroll To Top"
* **You will see a blank screen**
* Scroll down slightly with the mouse wheel and the items will appear.

Logs when clicking "Scroll To Top".
* The first MeasureOverride is given a RealizationRect that is adjusted for the LayoutOrigin.Y of -4255.822.
* The app picks the anchor of index 0 at Y=0.
* Nothing is rendered as this is far outside the viewport, but it does reset LayoutOrigin to 0,0.
* The next MeasureOverride has the new LayoutOrigin, but has not changed the RealizationRect to match so nothing is drawn.

```
MeasureOverride: RealizationRect = 0,-4255.822,939,836 LayoutOrigin = 0,-4255.822
GetAnchor: calculated new anchor 0, 0
MeasureLayout: Nothing laid out
GetExtent - No Items were laid out, resetting extent
MeasureOverride: RealizationRect = 0,-4255.822,939,836 LayoutOrigin = 0,0
GetAnchor: calculated new anchor 0, 0
MeasureLayout: Nothing laid out
GetExtent - No Items were laid out, resetting extent
```

Logs when scrolling down one mouse wheelclick
* The RealizationRect has been fixed to match the LayoutOrigin
* It scrolls down a few steps and everything renders correctly.

```
After one mouse wheel scroll down the RealizationRect is scrolled down about one click.
MeasureOverride: RealizationRect = 0,27.50977,939,836 LayoutOrigin = 0,0
GetAnchor: calculated new anchor 0, 0
MeasureLayout: 0 - 8
GetExtent: 0,0,939,124029.9
MeasureOverride: RealizationRect = 0,103.2031,939,836 LayoutOrigin = 0,0
GetAnchor: reusing previous anchor 0, 0
MeasureLayout: 0 - 8
GetExtent: 0,0,939,124029.9
MeasureOverride: RealizationRect = 0,125,939,836 LayoutOrigin = 0,0
GetAnchor: reusing previous anchor 0, 0
MeasureLayout: 0 - 8
GetExtent: 0,0,939,124029.9
```

## Expectation

I expected that, after the MeasureOverride returns with a new LayoutOrigin of 0,0, that the next call to MeasureOverride would adjust the RealizationRect by the same amount that the LayoutOrigin was adjusted.  This would reset the RealizationRect to 0,0 and the items would render correctly.
