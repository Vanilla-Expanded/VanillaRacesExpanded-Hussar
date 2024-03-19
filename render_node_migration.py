"""
Written by RedMattis 2024-01-27. Last updated 2024-01-31

This script will attempt to migrate your graphicData nodes in the xml files in the same directory as this script to the new renderNodeProperties format.
If will only run on "GeneDef" files. In theory you can simply remove that if-statement to run it on other nodes as well, but I have not tested if that works.

No guarentees are made that the resulting xml files will work in RimWorld. This script is provided as-is and it is not made by, or supported by Ludeon Studios.
If it breaks something and you didn't make backup copies of your xml files, you have my sympathies, and hopefully you learned to make backups.

To run the script simply put it in the folder you want to update XML files in. The script will then update all files in that folder and every subdirectory to an
infinite depth.
"""


import xml.etree.ElementTree as ET
import os
from xml.dom import minidom

# Examples of data, for reference.
# Example of the XML of a renderNodeProperties node.
"""
<renderNodeProperties>
  <li>
    <nodeClass>PawnRenderNode_AttachmentHead</nodeClass>
    <workerClass>PawnRenderNodeWorker_FlipWhenCrawling</workerClass>
    <texPath>Things/Pawn/Humanlike/HeadAttachments/PigNose/PigNose</texPath>
    <narrowCrownHorizontalOffset>0.03</narrowCrownHorizontalOffset>
    <useSkinShader>true</useSkinShader>
    <useRottenColor>true</useRottenColor>
    <colorType>Skin</colorType>
    <parentTagDef>Head</parentTagDef>
    <rotDrawMode>Fresh, Rotting</rotDrawMode>
    <visibleFacing>
      <li>East</li>
      <li>South</li>
      <li>West</li>
    </visibleFacing>
    <drawData>
      <defaultData>
        <layer>66</layer>
      </defaultData>
    </drawData>
  </li>
</renderNodeProperties>
"""
# Example of a GraphicData node.
"""
<graphicData>
  <drawLoc>Tailbone</drawLoc>
  <graphicPaths>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_default</li>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_child</li>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_fat</li>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_hulk</li>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_female</li>
    <li>Pawn/BodyAttachments/LoS_SnakeTail/bp_male</li>
  </graphicPaths>
  <drawScale>2.25</drawScale>
  <drawOffsetNorth>(0, 0.0150617733, -0.3067)</drawOffsetNorth>
  <drawOffsetSouth>(0, 0.0150617733, -0.3067)</drawOffsetSouth>
  <drawOffsetEast>(-0.3067, 0.0150617733, -0.3067)</drawOffsetEast>
  <colorType>Skin</colorType>
</graphicData>
"""

# Example of DrawData
"""
<drawData>
  <scaleOffsetByBodySize>true</scaleOffsetByBodySize>
  <useHediffAnchor>true</useHediffAnchor>
  <childScale>0.7</childScale>
  <bodyTypeScales>
    <Hulk>1.2</Hulk>
    <Fat>1.4</Fat>
    <Thin>0.8</Thin>
  </bodyTypeScales>
  <defaultData>
    <layer>49</layer>
  </defaultData>
  <dataNorth>
    <rotationOffset>310</rotationOffset>
    <flip>true</flip>
    <layer>0</layer>
  </dataNorth>
  <dataEast>
    <rotationOffset>270</rotationOffset>
    <flip>true</flip>
  </dataEast>
  <dataSouth>
    <rotationOffset>220</rotationOffset>
  </dataSouth>
  <dataWest>
    <rotationOffset>270</rotationOffset>
  </dataWest>
</drawData>
"""
SubInterval = 0.00038461538

class GraphicData:
  def __init__(self, defName, old_graphics_data, parent):
    self.old_data = old_graphics_data
    self.parent = parent

    # check if the parent has the XML-attribute "ParenName"
    self.grandparent_name = parent.get('ParentName')
    if self.grandparent_name:
      self.grandparent_name = self.grandparent_name.lower()

    # check if the parent has the XML-attribute Name
    self.parent_name = parent.get('Name')
    if self.parent_name:
      self.parent_name = self.parent_name.lower()
    # Check if the parent has a child with the tag "defName"
    elif parent.find('defName'):
      self.parent_name = parent.find('defName').text.lower()

    # Collect the data from the xml.
    self.defName = defName

    # Get the texPath
    self.texPath = self.get_text('graphicPath')
    self.texPaths = self.get_list_of_text('graphicPaths')
    self.texPathFemale = None #self.get_text('texPathFemale')
    self.texPathsFemale = None #self.get_list_of_text('texPathsFemale')

    self.graphicClass = self.get_text('graphicClass')

    self.layer = self.get_text('layer')

    # Get Drawscale
    self.drawScale = self.get_text('drawScale')

    # Get drawOffsetNorth, drawOffsetSouth, and drawOffsetEast.
    self.drawOffsetNorth = self.get_text('drawOffsetNorth')
    self.drawOffsetSouth = self.get_text('drawOffsetSouth')
    self.drawOffsetEast = self.get_text('drawOffsetEast')

    # Colordata
    self.colorType = self.get_text('colorType')
    self.useSkinShader = self.get_text('useSkinShader')
    self.color = self.get_text('color')

    # Draw Location
    self.drawLoc = self.get_text('drawLoc')

    self.narrowCrownHorizontalOffset = self.get_text('narrowCrownHorizontalOffset')


  def get_text(self, tag):
    element = self.old_data.find(tag)
    return element.text if element is not None else None
  
  def get_list_of_text(self, tag):
    # Get the element and then make a list from each sub-element
    element = self.old_data.find(tag)
    return [e.text for e in element] if element is not None else []
  
  def build_PawnRenderNodeProperties(self):
    prn_properties = PawnRenderNodeProperties(self)
    return prn_properties

class PawnRenderNodeProperties:
  # Suitable for facial features, e.g. "Brow_Heavy"
  DEFAULT_HEAD_DD = {
    "drawData":
    {
      "defaultData":
      {
        "layer": 52
      },
    }
  }
  # Suitable for horns. Will make them poke through helmets. Probably.
  HORN_DD = {
    "drawData":
    {
      "defaultData":
      {
        "layer": 80
      },
      "dataNorth":
      {
        "layer": 10
      }
    }
  }
  WING_DD = {
    "drawData":
    {
      "defaultData":
      {
        "layer": 10
      },
      "dataNorth":
      {
        "layer": 80
      }
    }
  }
  Tail_DD = {
    "drawData":
    {
      "defaultData":
      {
        "layer": -2
      },
    }
  }

  def __init__(self, graphic_data):
    self.data_dict = {}
    # Check if the graphicData is a head attachment or not.
    is_head_attachment = self.is_head_attachment(graphic_data)
      
    # Classes
    if is_head_attachment:
      self.data_dict["nodeClass"] = "PawnRenderNode_AttachmentHead"
      self.data_dict["workerClass"] = "PawnRenderNodeWorker_FlipWhenCrawling"
    else:
      self.data_dict["parentTagDef"] = "Body"
      # self.data_dict["workerClass"] = "PawnRenderNodeWorker_AttachmentBody"  # Needs testing.

    # Texture Paths
    self.data_dict["texPath"] = graphic_data.texPath
    self.data_dict["texPaths"] = graphic_data.texPaths
    self.data_dict["texPathFemale"] = graphic_data.texPathFemale
    self.data_dict["texPathsFemale"] = graphic_data.texPathsFemale

    # Draw Size
    if graphic_data.drawScale:
      drawScale = (float)(graphic_data.drawScale)
      drawSize = (drawScale - 1) * 0.5 + 1

      self.data_dict["drawSize"] = f"({drawSize},{drawSize})"
      #self.data_dict["overrideMeshSize"] = f"({graphic_data.drawScale},{graphic_data.drawScale})"

    # Color
    self.data_dict["color"] = graphic_data.color
    self.data_dict["colorRGBPostFactor"] = None
    self.data_dict["useRottenColor"] = None
    self.data_dict["useSkinShader"] = graphic_data.useSkinShader
    self.data_dict["shaderTypeDef"] = None
    self.data_dict["colorType"] = graphic_data.colorType

    # Layer
    self.data_dict["baseLayer"] = None
    self.data_dict["drawData"] = None

    # DrawData

    # Check for tags. Skipping grandparent since it might be a bit too broad.
    is_horn = self.parent_has_matching_tag(graphic_data, ["horn", "antennae", "antenna"]) or self.defName_has_matching_tag(graphic_data, ["horn", "antennae", "antenna"])
    is_wing = self.parent_has_matching_tag(graphic_data, ["wing"]) or self.defName_has_matching_tag(graphic_data, ["wing"])
    is_tail = self.parent_has_matching_tag(graphic_data, ["tail"]) or self.defName_has_matching_tag(graphic_data, ["tail"])


    add_draw_data = graphic_data.drawOffsetNorth or graphic_data.drawOffsetSouth or graphic_data.drawOffsetEast or \
      is_horn or is_wing or is_tail

    no_defaults = False
    if is_horn:
      self.data_dict["drawData"] = PawnRenderNodeProperties.HORN_DD["drawData"]
      self.data_dict["parentTagDef"] = "Head"
    elif is_wing:
      self.data_dict["drawData"] = PawnRenderNodeProperties.WING_DD["drawData"]
    elif is_tail:
      self.data_dict["drawData"] = PawnRenderNodeProperties.Tail_DD["drawData"]
    elif is_head_attachment:
      self.data_dict["drawData"] = PawnRenderNodeProperties.DEFAULT_HEAD_DD["drawData"]
      self.data_dict["parentTagDef"] = "Head"
    else:
      no_defaults = True
      
    if add_draw_data:
      if not self.data_dict.get("drawData"):
        self.data_dict["drawData"] = {}
      # if not is_head_attachment:
      #self.data_dict["drawData"]["scaleOffsetByBodySize"] = True
      if graphic_data.drawOffsetNorth:
        if not self.data_dict["drawData"].get("dataNorth"):
          self.data_dict["drawData"]["dataNorth"] = {}
        if no_defaults:
          self.data_dict["drawData"]["dataNorth"]["offset"] = graphic_data.drawOffsetNorth
        else:
          self.data_dict["drawData"]["dataNorth"]["offset"] = self.drawOffset_xz(graphic_data.drawOffsetNorth)
        #self.data_dict["drawData"]["dataNorth"]["layer"] = self.drawOffset_to_layer(graphic_data.drawOffsetNorth)
      if graphic_data.drawOffsetSouth:
        if not self.data_dict["drawData"].get("dataSouth"):
          self.data_dict["drawData"]["dataSouth"] = {}
        if no_defaults:
          self.data_dict["drawData"]["dataSouth"]["offset"] = graphic_data.drawOffsetSouth
        else:
          self.data_dict["drawData"]["dataSouth"]["offset"] = self.drawOffset_xz(graphic_data.drawOffsetSouth)
        #self.data_dict["drawData"]["dataSouth"]["layer"] = self.drawOffset_to_layer(graphic_data.drawOffsetSouth)
      if graphic_data.drawOffsetEast:
        if not self.data_dict["drawData"].get("dataEast"):
          self.data_dict["drawData"]["dataEast"] = {}
        if no_defaults:
          self.data_dict["drawData"]["dataEast"]["offset"] = graphic_data.drawOffsetEast
        else:
          self.data_dict["drawData"]["dataEast"]["offset"] = self.drawOffset_xz(graphic_data.drawOffsetEast)
        #self.data_dict["drawData"]["dataEast"]["layer"] = self.drawOffset_to_layer(graphic_data.drawOffsetEast)

    # Other
    self.data_dict["narrowCrownHorizontalOffset"] = graphic_data.narrowCrownHorizontalOffset

    # Unset
    self.data_dict["rotDrawMode"] = None
    self.data_dict["visibleFacing"] = None
  
  def defName_has_matching_tag(self, graphic_data, tag_list):
    return graphic_data.defName and any(keyword in graphic_data.defName for keyword in tag_list)

  def parent_has_matching_tag(self, graphic_data, tag_list):
    return graphic_data.parent_name and any(keyword in graphic_data.parent_name for keyword in tag_list)
  
  def grandparent_has_matching_tag(self, graphic_data, tag_list):
    return graphic_data.grandparent_name and any(keyword in graphic_data.grandparent_name for keyword in tag_list)
  
  def is_head_attachment(self, graphic_data):
    head_keywords_to_look_for = ["head", "horn", "tusk", "jaw", "eye", "ear", "nose", "mouth", "neck", "mandible", "beak", "antenna", "scalp", "skull", "hair"]
    if graphic_data.layer == "PostHeadgear":
      return True
    # If the drawLoc is head... Then Yes.
    elif graphic_data.drawLoc and "head" in graphic_data.drawLoc.lower():
      return True
    elif self.parent_has_matching_tag(graphic_data, head_keywords_to_look_for) \
      or self.grandparent_has_matching_tag(graphic_data, head_keywords_to_look_for) \
      or self.defName_has_matching_tag(graphic_data, head_keywords_to_look_for):
      return True
    return False

  # This stuff is wrong, ignore it.
  def drawOffset_xz(self, drawOffset):
    """
    Gets the X & Z coordinates of the drawOffset.
    """
    if drawOffset:
      # parse string to tuple
      drawOffset_tuple = eval(drawOffset)
      # save x & z into new string.
      return f"({drawOffset_tuple[0]}, 0, {drawOffset_tuple[2]})"
  
  def get_height(self, drawOffset):
    """
    Gets distance towards camera.
    E.g. (-0.70, 0.05, 0.011) to 0.05
    """
    if drawOffset:
      drawOffset_tuple = eval(drawOffset)
      return drawOffset_tuple[1]
    
  def drawOffset_to_layer(self, drawOffset):
    """
    Converts a drawoffset float to a layer integer.

    altitude = Mathf.Clamp(layer, -10, 100) * SubInterval
    (SubInterval = 0.00038461538)
    """
    drawOffset_tuple = eval(drawOffset)
    altitude = drawOffset_tuple[1]
    return int(altitude / SubInterval)


  def generate_xml_node(self, parent):
    """
    This method generates an xml. Any settings set to None or empty array will be skipped.
    """
    root = ET.Element('renderNodeProperties')
    # Add an attirbute to the root node called "Inherit" with the value "False"
    root.set('Inherit', 'False')

    # Create an "li" node and parent it to the root
    li_node = ET.SubElement(root, 'li')
      
    for key, value in self.data_dict.items():
      self.try_add_node(li_node, key, value)
    return root

  
  def try_add_node(self, parent, tag, data):
      """
      Will add a node to the parent if the data is not None or an empty array.

      Dictionaries will create a node with child nodes where the keys are the tags and the values are the data. If the value is a list or dictionary, it
      will be added as a sub-node instead.

      Lists will add sub-nodes under them, each with the tag "li" and the list entries data. ("li" is short for "list item")
      """
      if data:
        if isinstance(data, dict):
          # If the data is a dictionary, create a new node with the tag and add it to the parent.
          node = ET.SubElement(parent, tag)
          for key, value in data.items():
            # Recursively call try_add_node for nested dictionaries or lists.
            self.try_add_node(node, key, value)
        elif isinstance(data, list):
          # If the data is a list, create sub-nodes with the tag "li" for each entry and add them to the parent.
          node = ET.SubElement(parent, tag)
          for item in data:
            li_node = ET.SubElement(node, 'li')
            li_node.text = str(item)
        else:
          # If the data is a simple value, create a new node with the tag and set the text content.
          node = ET.SubElement(parent, tag)
          node.text = str(data)

script_path = os.path.dirname(os.path.realpath(__file__))
def edit_files(starting_directory):
  # set the directory of this file to the starting directory.


  # Step through all the files in the directory and subdirectories and collect the ones ending in .xml
  # and put them in a list.
  xml_files = []
  for root, dirs, files in os.walk(starting_directory):
    for file in files:
      if file.endswith(".xml"):
        xml_files.append(os.path.join(root, file))

  edited_files = []
  # Step through the list of xml files and open each one.
  for xml_file in xml_files:
    tree = ET.parse(xml_file)
    root = tree.getroot()
    g_data_found = False
    
    # Step through the xml nodes and collect all nodes with the name "graphicData".
    graphicData_nodes = []
    for graphicData in root.iter('graphicData'):

      # Figure out the parent of the node (there is no parent attribute in the xml node, so we have to check what item has this node as a child)
      parent = None
      for item in root.iter():
        if graphicData in item:
          parent = item
          break

      if parent.tag == "GeneDef":
        # Get the defName of the parent if it has one.
        p_defName = parent.find('defName')
        if p_defName is not None:
          defName = p_defName.text.lower()
        else:
          defName = None

        # Create a GraphicData object
        graphic_data_obj = GraphicData(defName, graphicData, parent)
        
        # Create a PawnRenderNodeProperties object
        prn_properties = graphic_data_obj.build_PawnRenderNodeProperties()

        # Generate updated XML
        updated_xml_root = prn_properties.generate_xml_node(parent)

        # Replace the existing graphicData node with the new XML
        parent.remove(graphicData)
        parent.append(updated_xml_root)
        g_data_found = True

    if g_data_found:
      # Pretty print the XML
      xml_string =  minidom.parseString(ET.tostring(root)).toprettyxml(indent="  ")
      xml_lines = [line for line in xml_string.split('\n') if line.strip()]
      formatted_xml = '\n'.join(xml_lines)
      with open(xml_file, "w") as f:
        f.write(formatted_xml)

      # print the formated xml for debugging.
      print(formatted_xml)
      edited_files.append(xml_file)



  # Print the list of edited files seperated by newlines.
  edited_files_count = len(edited_files)
  print("\n".join(edited_files))

  # Pause
  input("Iterrated over {} files.\nPress Enter to continue...".format(edited_files_count))

def query_run_path():
  print("Please supply a path to run the script on.")
  path = input()
  edit_files(path)

  print("Would you like to run the script again? (Y/N)")
  answer = input().lower()
  if answer == "y":
    query_run_path()

# Query the user if they would like to run the script on the current directory.
def query_run():
  # Ask the user if they would like to run the script on the current directory.
  print("Would you like to run the script on the current directory? (Y/N)\nIf not you will be asked to supply a path.")
  answer = input().lower()
  if answer == "y":
    edit_files(script_path)
  elif answer == "n":
    print("Please supply a path to run the script on.")
    path = input()
    edit_files(path)

    # Ask the user if they would like to run the script again.
    print("Would you like to run the script again? (Y/N)")
    answer = input().lower()
    if answer == "y":
      query_run_path()

# Run the script.
query_run()

