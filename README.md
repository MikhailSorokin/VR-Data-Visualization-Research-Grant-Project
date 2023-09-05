# VR-Data-Visualization-Research-Grant-Project
NOTE: This project uses a very old version of Unity and older headsets - Unity 5.X

A Virtual Reality data visualization research grant project.

How to use this visualization: 

This project is compatible with XML data so download a dataset from DBLP and this should be able to parse it.

However, if you push a change with the XML file, the gitignore file will make sure that
the XML file won't be pushed with the rest of the changes. 

Also IMPORTANT, place this file in the ROOT directory of the github project, once you have downloaded it, and AFTER
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

Once you have typed in the author, press enter on the keyboard or press down on either controller's touchpad to load in the connections.

IMPORTANT - make sure the title of the GUI is "Coauthors" or else connections will not be drawn.

The current connections shown are between the inputted author and all of his coauthors and coauthors' coauthors. As in,
the connections go "one hop out" and finds connections of coauthor as well, rather than direct connections. This is due to the way our 
visualization is currently set up, in which the lowest level has datapoints as articles, rather
than authors. We are in the process of changing this to have the lowest level be authors and the lines connecting between 
the authors being direct (1st degree) connections to represent coauthor relationships.

If you wish to remove the connections from your current view, just press switch on the menu GUI and it will clean all the connections. You can then redraw connections again by switching back to the "Coauthors" title and repeating the steps above.

Right now, the connecting splines are spread out from each other and it can be hard to see where lines are being drawn to and from. This is something that we, or anyone else interested, can explore.

# Alternative Input Options
With the current way of doing input, one will usually have to take off the headset to see the keyboard. So, it will be cool if there are some ways to explore input in VR. One suggestion could be using the camera on the Vive headset. If a collaborator would like to explore this option, that would be awesome.

