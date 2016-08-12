# VR-Data-Visualization-Research-Grant-Project
A Virtual Reality data visualization research grant project.

How to use this visualization: 

IMPORTANT - The first thing to do is download the DBLP dataset from here: https://www.dropbox.com/sh/qyf90qe3yxon2xi/AAAQzeRPpmJ-1lxhUFWavH8ba?dl=0.
Warning, this file is pretty large (about 1 GB of space) and I couldn't
include it as part of the Github repository, so it will have to be like this for now.
However, if you push a change with the XML file, the gitignore file will make sure that
the XML file won't be pushed with the rest of the changes. 
Also IMPORTANT, Place this file in the Assets directory once you have downloaded it and AFTER
you have opened the project at least once (metadata needs to be loaded/assets need to be generated and such)!

Controls will be shown on the GUI displays in game.
Once you have loaded in the data, you can interact with the datapoints
by putting either the left or right controller over any datapoint. 

Once you have put the controller over the datapoint, you can press the
trigger to expand the datapoint to a ring on a lower level.

# Connections
If you wish to draw connections between authors, you have to press switch on the GUI to go to the "Coauthors" 
section, and manually type in the author of which you would like to see connections drawn to and from.

Here are sample authors to use for testing connections:
- Joost Engelfriet
- Christian Lengauer

You can find more interesting authors if you take a look through the XML file.

Once you have typed in the author, press enter on the keyboard or press down on the touchpad to load in the connections.
IMPORTANT - make sure the title of the GUI is "Coauthors" or else connections will not be drawn.

This will show connections between the coauthors' articles and all of the other inputted author's connections. As in,
it goes two degrees and finds connections of the coauthor, rather than direct connections due to the way our 
visualization is currently set up. The reason for this is because the lowest level has datapoints as articles, rather
than authors. We are in the process of changing this to have the lowest level be authors and lines connecting between 
the authors being direct (1st degree) connections to represent coauthor relationship.

If you wish to remove the connections from your current view, just press switch on the menu GUI and it will clean all the connections. You can then redraw connections again by switching back and repeating the steps, listed under the connections section.

# Alternative Input Options
With the current way of doing input, one will usually have to take off the headset to see the keyboard. So, it will be cool if there are some ways to explore input in VR. One suggestion could be using the camera on the Vive headset. If a collaborator would like to explore this option, that would be awesome.

