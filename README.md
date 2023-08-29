# DICOM Editor

An implementation of DICOM editor that allows DICOM instances to be retrieved from DICOM server or imported from local storage. The instances can be edited by modifying a value of an existing attribute, deleting an attribute, and adding a new attribute. The edited instances can be stored to DICOM server or exported to local storage.
The editor was tested with Orthanc (open-source DICOM server).

## Features
### Import view:
- Query DICOM server
	- Filters (patient ID, patient name, accession number, study ID, modality)
- Retrieve from DICOM server
	- Select a patient to retrieve all studies and consequently all series in the studies
	- Select a study to retrieve all series in the selected study
	- Select a series to retrieve the selected series
- Import from disk
	- Input a path to a folder (sub directories are not imported)
	- Input a path to a single file
    
![image](https://user-images.githubusercontent.com/48628230/202871976-f1dfa877-1a83-47f0-b293-aa63f9bc34e2.png)


### Editor view:
- Series list (series descriptions are displayed)
	- Select the desired series
	- Next to each series that contains images there is a button to display the images.
- Instances list (instance numbers are displayed; if instance number is missing instance UID is displayed)
	- Select the desired instance from the previously selected series
	- Next to each instance that is an image there is a button to display the image.
	- Next to each instance there is a button to delete the instance.
- Search the attribute table based on the entered search term. The tags or values that partially or completely match the search term are marked blue.
- Attributes table
	- Select an attribute to:
		- Modify its value
		- Delete it
		- Add a new attribute/sequence on the same level as the selected attribute
	- Select a sequence to:
		- Delete it
		- Add a new empty item
	- Select an item in a sequence to:
		- Delete it
		- Add a new attribute/sequence
- Generate a study UID for the selected instance
	- If DICOM root is configured the root is used for generating the UID, otherwise a "2.25...." UID is generated
- Generate a series UID for the selected instance
	- If DICOM root is configured the root is used for generating the UID, otherwise a "2.25...." UID is generated
	- **NOTE:** since instances are grouped by series, changing the series UID of an instance will create a new series in the series list, if the series does not exist yet.
- Generate an instance UID for the selected instance
	- If DICOM root is configured the root is used for generating the UID, otherwise a "2.25...." UID is generated
- Apply an action to all instances in the selected series
	- **NOTE:** in this case, "generate instance UID" behaves slightly different than generating study or series UID. Generate instance UID will create a new UID for each instance in the series whereas generating study or series UID will generate one UID and apply it to all instances in the series.
	- **NOTE2:** while adding individual items and/or attributes it can happen that not all instances have the same items and/or attributes. In this case, applying a series-wide change to such an attribute will result in a notification containing a message saying that index was out of range.
- Validate attributes of the selected instance
	- If the option is turned on, instance gets automatically validated upon selection.
	- Attributes, sequences and sequence items that fail validation are highlighted in red.
- Store selected series to DICOM server
- Export selected series to disk
	- Input a target path; if a folder does not exist, it is created automatically
  
![image](https://github.com/agerecnik/dicom-editor/assets/48628230/ce1431b6-3780-4282-ab51-567c7cfd7947)

![image](https://github.com/agerecnik/dicom-editor/assets/48628230/0b5c5d3d-b821-4155-b939-f44e3efd02b7)



### Settings view:
- Configure query/retrieve remote AE
- Configure store remote AE
- C-ECHO for each remote AE
- Configure application AET
- Configure DICOM root
  - If DICOM root is present UIDs are generated in the following way:
  	`<root>.<year><month><day><hour><minute><second><millisecond>.<type>.<counter>.<random>`
    - Legend:
      - year: 4 characters
      - month: 2 characters
      - day: 2 characters
      - hour: 2 characters
      - minute: 2 characters
      - second: 2 characters
      - millisecond: 3 characters
      - type: 1 character (1 = study, 2 = series, 3 = instance)
      - counter: depends on the number of instances that are being changed in one run (e.g., for 1000 instances it would be 3 characters (0 - 999))
        - **NOTE:** for studies and series it is always one character (0)
      - random: a random number of up to 5 characters
    - **NOTE:** if you already use your root for any other Application Entity you must also ensure to include a unique ID in the scope of said root for each instance of DICOM editor with whom you are going to use the root: `<root>.<ID>.`
    - **NOTE2:** don't forget to add the separator `.` at the end of the root.
    - **NOTE3:** bear in mind that the maximum allowed length of UIDs is 64 characters, and that separators `.` also count as characters.

![image](https://user-images.githubusercontent.com/48628230/202870504-f946311a-f5ad-434d-97b1-e198e078eb7f.png)
