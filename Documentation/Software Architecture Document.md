







*Software Architecture Document




*Master Repository of Computing History Artifacts Information

*MARECHAI

Natalia Portillo



Version 1.3


2020/06/12

**Revision History**


| **Version** | **Description of Versions / Changes** | **Responsible Party** | **Date** |
| :-: | :-: | :-: | :-: |
| 1.0 | Initial version | Natalia Portillo | 2019/08/18 |
| 1.1 | Renamed to Marechai and DiscImageChef renamed to Aaru | Natalia Portillo | 2020/05/23 |
| 1.2 | Changed hash database types to use binary instead of string. Defined file attributes. Defined subchannel flags. Defined dump status flags. Redefine standalone file as derivate of media file. Some minor fixes. | Natalia Portillo | 2020/06/11 |
| 1.3 | Change software by media by magazine to software by compilation media. | Natalia Portillo | 2020/06/12 |



**Approval Block**

| **Version** | **Comments** | **Responsible Party** | **Date** |
| :-: | :-: | :-: | :-: |
|  |  |  |  |
|  |  |  |  |
|  |  |  |  |
|  |  |  |  |



Table of Contents



[1. Introduction	1](#__RefHeading___Toc2731_464595390)

[1.1. Purpose	1](#__RefHeading___Toc2733_464595390)

[1.2. Scope	1](#__RefHeading___Toc2735_464595390)

[1.3. Definitions, Acronyms, and Abbreviations	1](#__RefHeading___Toc2737_464595390)

[1.4. History	2](#__RefHeading___Toc2739_464595390)

[1.5. Overview	2](#__RefHeading___Toc2741_464595390)

[2. Architectural Goals and Constraints	3](#__RefHeading___Toc2743_464595390)

[3. Description of systems	4](#__RefHeading___Toc2745_464595390)

[3.1. Main systems	4](#__RefHeading___Toc2747_464595390)

[3.1.1. Web application	4](#__RefHeading___Toc2749_464595390)

[3.1.2. Mobile application	4](#__RefHeading___Toc2751_464595390)

[3.1.3. Metadata sidecar	4](#__RefHeading___Toc2753_464595390)

[3.2. Sister projects	4](#__RefHeading___Toc2755_464595390)

[3.2.1. Aaru	4](#__RefHeading___Toc2757_464595390)

[4. Web application	5](#__RefHeading___Toc2759_464595390)

[4.1. User	5](#__RefHeading___Toc2761_464595390)

[4.2. Role	5](#__RefHeading___Toc2938_464595390)

[4.2.1. Uberadmin	5](#__RefHeading___Toc2940_464595390)

[4.2.2. Writer	5](#__RefHeading___Toc2942_464595390)

[4.2.3. Proofreader	5](#__RefHeading___Toc2944_464595390)

[4.2.4. Translator	6](#__RefHeading___Toc2946_464595390)

[4.2.5. Supertranslator	6](#__RefHeading___Toc2948_464595390)

[4.2.6. Collaborator	6](#__RefHeading___Toc2950_464595390)

[4.2.7. Curator	7](#__RefHeading___Toc2952_464595390)

[4.2.8. Physical curator	7](#__RefHeading___Toc2954_464595390)

[4.2.9. Technician	7](#__RefHeading___Toc2956_464595390)

[4.2.10. Supertechnician	7](#__RefHeading___Toc2958_464595390)

[4.3. User view	8](#__RefHeading___Toc2938_4645953901)

[4.4. Admin view	8](#__RefHeading___Toc1773_2397447227)

[5. Artifacts	9](#__RefHeading___Toc1775_2397447227)

[5.1. Book	9](#__RefHeading___Toc6212_464595390)

[5.2. Companies by book	9](#__RefHeading___Toc6214_464595390)

[5.3. Companies by document	10](#__RefHeading___Toc6216_464595390)

[5.4. Companies by magazine	10](#__RefHeading___Toc6218_464595390)

[5.5. Company	10](#__RefHeading___Toc6420_464595390)

[5.6. Company description	11](#__RefHeading___Toc1779_2397447227)

[5.7. Company logo	11](#__RefHeading___Toc1781_2397447227)

[5.8. Currency inflation	12](#__RefHeading___Toc6956_464595390)

[5.9. Currency pegging	12](#__RefHeading___Toc6958_464595390)

[5.10. Document	12](#__RefHeading___Toc6220_464595390)

[5.11. Document company	13](#__RefHeading___Toc6222_464595390)

[5.12. Document person	13](#__RefHeading___Toc6224_464595390)

[5.13. Document role	14](#__RefHeading___Toc6226_464595390)

[5.14. Dump	14](#__RefHeading___Toc22116_2414510537)

[5.15. Dump hardware	15](#__RefHeading___Toc10534_2414510537)

[5.16. Extent	15](#__RefHeading___Toc22118_2414510537)

[5.17. File	16](#__RefHeading___Toc7014_464595390)

[5.18. File data stream	16](#__RefHeading___Toc22120_2414510537)

[5.19. Filesystem	17](#__RefHeading___Toc22122_2414510537)

[5.20. Graphical Processing Unit	18](#__RefHeading___Toc6422_464595390)

[5.21. Instruction set	18](#__RefHeading___Toc6228_464595390)

[5.22. Instruction set extension	18](#__RefHeading___Toc6230_464595390)

[5.23. ISO 3166-1 Numeric	19](#__RefHeading___Toc6232_464595390)

[5.24. ISO 4217	19](#__RefHeading___Toc6960_464595390)

[5.25. ISO 639	19](#__RefHeading___Toc6234_464595390)

[5.26. License	20](#__RefHeading___Toc6236_464595390)

[5.27. Logical partition	20](#__RefHeading___Toc22124_2414510537)

[5.28. Machine	21](#__RefHeading___Toc6238_464595390)

[5.29. Machine family	22](#__RefHeading___Toc6240_464595390)

[5.30. Machine photo	22](#__RefHeading___Toc6424_464595390)

[5.31. Magazine	24](#__RefHeading___Toc6426_464595390)

[5.32. Magazine issue	24](#__RefHeading___Toc6428_464595390)

[5.33. Mastering texts by media	25](#__RefHeading___Toc22126_2414510537)

[5.34. Media	25](#__RefHeading___Toc22128_2414510537)

[5.35. Media dump	27](#__RefHeading___Toc22130_2414510537)

[5.36. Media dump file image	27](#__RefHeading___Toc22132_2414510537)

[5.37. Media dump image	27](#__RefHeading___Toc10540_2414510537)

[5.38. Media dump track image	28](#__RefHeading___Toc10544_2414510537)

[5.39. Media dump subchannel image	29](#__RefHeading___Toc22134_2414510537)

[5.40. Media file	29](#__RefHeading___Toc22136_2414510537)

[5.41. Media tag dump	30](#__RefHeading___Toc22138_2414510537)

[5.42. Memory	30](#__RefHeading___Toc6430_464595390)

[5.43. Owned machine	31](#__RefHeading___Toc6432_464595390)

[5.44. Owned machine photo	32](#__RefHeading___Toc6434_464595390)

[5.45. People by book	34](#__RefHeading___Toc6436_464595390)

[5.46. People by document	34](#__RefHeading___Toc6216_4645953901)

[5.47. People by magazine	34](#__RefHeading___Toc6218_4645953901)

[5.48. People	34](#__RefHeading___Toc6438_464595390)

[5.49. Processor	35](#__RefHeading___Toc6440_464595390)

[5.50. Processor by machine	37](#__RefHeading___Toc6442_464595390)

[5.51. Processor by owned machine	37](#__RefHeading___Toc6444_464595390)

[5.52. Resolution	38](#__RefHeading___Toc6446_464595390)

[5.53. Screen	38](#__RefHeading___Toc6532_464595390)

[5.54. Software family	39](#__RefHeading___Toc22140_2414510537)

[5.55. Software variant	39](#__RefHeading___Toc22142_2414510537)

[5.56. Software version	41](#__RefHeading___Toc22144_2414510537)

[5.57. Sound synthetizer	42](#__RefHeading___Toc6534_464595390)

[5.58. Standalone files	43](#__RefHeading___Toc22146_2414510537)

[5.59. Storage by machine	43](#__RefHeading___Toc6536_464595390)

[5.60. Storage by owned machine	43](#__RefHeading___Toc6538_464595390)

[5.61. Software variant by compilation	43](#__RefHeading___Toc22148_2414510537)

[5.62. Table of contents	44](#__RefHeading___Toc22150_2414510537)

[5.63. Variable block size	44](#__RefHeading___Toc22027_2414510537)

[6. Enumerations	45](#__RefHeading___Toc6854_464595390)

[6.1. Status type	45](#__RefHeading___Toc6212_4645953901)

[6.2. Company status	45](#__RefHeading___Toc6588_464595390)

[6.3. Machine type	45](#__RefHeading___Toc6590_464595390)

[6.4. Memory type	46](#__RefHeading___Toc6592_464595390)

[6.5. Memory usage	47](#__RefHeading___Toc6742_464595390)

[6.6. Storage type	48](#__RefHeading___Toc6744_464595390)

[6.7. Storage interface	54](#__RefHeading___Toc6746_464595390)

[6.8. ColorSpace	55](#__RefHeading___Toc6748_464595390)

[6.9. Contrast	55](#__RefHeading___Toc6750_464595390)

[6.10. ExposureMode	56](#__RefHeading___Toc6752_464595390)

[6.11. ExposureProgram	56](#__RefHeading___Toc6754_464595390)

[6.12. FlashMode	56](#__RefHeading___Toc6756_464595390)

[6.13. LightSource	57](#__RefHeading___Toc6758_464595390)

[6.14. MeteringMode	58](#__RefHeading___Toc6760_464595390)

[6.15. Orientation	59](#__RefHeading___Toc6762_464595390)

[6.16. ResolutionUnit	59](#__RefHeading___Toc6764_464595390)

[6.17. Saturation	60](#__RefHeading___Toc6766_464595390)

[6.18. SceneCaptureType	60](#__RefHeading___Toc6768_464595390)

[6.19. SensingMethod	60](#__RefHeading___Toc6770_464595390)

[6.20. SubjectDistanceRange	60](#__RefHeading___Toc6772_464595390)

[6.21. WhiteBalance	61](#__RefHeading___Toc6774_464595390)

[6.22. Sharpness	61](#__RefHeading___Toc6776_464595390)

[6.23. Mastering text type	61](#__RefHeading___Toc11389_3751602060)

[6.24. Media type	62](#__RefHeading___Toc11642_3751602060)

[6.25. Dump status flags	80](#__RefHeading___Toc11928_3751602060)

[6.26. Subchannel flags	81](#__RefHeading___Toc12220_3751602060)

[6.27. File attributes	82](#__RefHeading___Toc12515_3751602060)

[6.28. Media tag type	84](#__RefHeading___Toc12518_3751602060)

[6.29. Distribution mode	84](#__RefHeading___Toc12518_37516020601)

[6.30. SoundSynthType	84](#__RefHeading___Toc12945_3751602060)

[6.31. TrackType	84](#__RefHeading___Toc12948_3751602060)

[7. Examples	86](#__RefHeading___Toc22029_2414510537)



# Introduction

This document provides a high level overview and explains the architecture of the Marechai systems (that is, web and mobile applications, as well as metadata and database schemas).


The document defines goals of the architecture, the use cases supported by the system, architectural styles and components that have been selected. The document provides a rationale for the architecture and design decisions made from the conceptual idea to its implementation.  


## Purpose


The Software Architecture Document (SAD) provides a comprehensive architectural overview of the Master Repository of Computing History Artifacts Information (Marechai). It presents a number of different architectural views to depict the different aspects of the system.  


## Scope


The scope of this SAD is to explain the architecture of the Distributed Development Monitoring and Mining system. 


This document describes the various aspects of the Marechai systems design that are considered to be architecturally significant. These elements and behaviors are fundamental for guiding the construction of the Marechai systems and for understanding this project as a whole.


## Definitions, Acronyms, and Abbreviations


- **Artifact: Refers to any element registered in the database. It can be physical, or digital only.**

- **Component: The physical element that compose a physical artifact.**

- **Dump: The digital representation of the contents of a software artifact. Can be generated from a physical artifact (e.g. an installation disc, or a game cartridge), or from a digital artifact (e.g. an installation package).**

- **Inventory: The cataloguing of physical artifacts belonging to an entity like a museum, society or collector.**

- **Machine: A turing-complete physical artifact, existing or at least prototyped, that allows the execution of software, user or manufacturer provided. It englobes typical concepts like computers, videogame consoles, arcade systems, smartphones, etc.**

- **Screenshot: An image representing the execution of a software artifact.**

 

## **History**


The Marechai project started in 2002 as a database of old computer specification, implemented in PHP. The second version of the database and front end was launched publicly the 24th of December of 2003 in [http://museum.claunia.com](http://museum.claunia.com/), with moderate success. In 2004 the third version started development, but it halted due to the lack of resources.


Since then, the launch of the Wikipedia, and several computer museums, with their own websites, changed the fundamental needs for the archival and representation of information about the history of computing.


In 2017 the new version of the Marechai database was designed and started to be implemented in ASP.NET at a slow pace. This is the version described in this document.



## Overview


In order to fully document all the aspects of the architecture, the Software Architecture Document contains the following subsections.

Section 2: to be filled


# Architectural Goals and Constraints

There are some key requirements and system constraints that have a significant bearing on the architecture.  They are:



1. All the systems will be in an OSI certified open source license. However it’s development will  be kept private to the developers until it arrives a phase secure enough for other users to experiment with it without complaining about known issues, or missing features, that are already in the backlog.


2. The web application will be implemented using the C\# language for the backend, and JavaScript for the frontend. It is to run under .NET Core in a Linux environment or Docker container.


3. The mobile applications will be implemented using the C\# language under the Xamarin frameworks.


4. The web frontend should not use any bloated framework (like Angular or Vue) but can use libraries or frameworks like Bootstrap and jQuery.


5. All the systems should be user agnostic, so they can be used by any museum or private collector.



# **Description of systems**

The purpose of this section is to describe the various systems that comprise the Marechai projects, as well as sister projects that help fullfil the same objectives.


## **Main systems**


### Web application

**The web application system shall provide end users with a view and description of computing history artifacts, including but not limited to, computing companies facts and history, machines and components specifications and photos, software descriptions and screenshots, and important persons history.**


**It shall also provide internal users, like museum curators and technicians, with a private asset inventory, repair log and auditing system.**


### Mobile application

**	The mobile application system shall provide end users access to the public part of the web application, in an offline way, similar to other applications like MacTracker or Intel ARK.**


### Metadata sidecar

**	The metadata sidecar specification is the specification for a file that contains several metadata about a digital artifact, that can accompany copies of it, and contains information about the artifact, its contents, and how it was obtained.**


**	It is currently hosted at [https://www.github.com/claunia/CICMMetadata](https://www.github.com/claunia/CICMMetadata) and supported for creation of media dumps by Aaru.**


## **Sister projects**


### **Aaru**

**	Aaru is an opensource application designed as a complete management tool of media dumps. It can create dumps from physical media, analyze, convert and hash them, and in a few cases, list the files contained in such dumps.**


**	It is, as of the writing of this document, the only known implementation of the Lisa filesystem, besides the Lisa operating system itself, and was used by the Computer History Museum to recover the historic source code of the Lisa operating system.**


**	It is currently hosted at [https://www.github.com/a](https://www.github.com/aaru-dps)aru-dps. **



# Web application

This section describes the components that make the web application system and their relationships.


## User

A user is any person that access the application. It can be an anonymous user, that is, not logged in, limited to read-only access to the information in the database, or a user with role, whose access to certain parts of the database is determined by their role.

Also the parts of the database accessible by anonymous users shall be configurable by the system administrator.

Any anonymous user shall be able to register, getting an automatic role as defined by the system administrator, and any registered and logged in user shall have the option to remove its account.

All personal data of a registered user shall comply with the GDPR rules, that is, but not limited to, the data must be stored encrypted at rest, not shared with third parties unless explicitly allowed by the user, and removed completely and irrecoverabily at the user will.

User login is controlled by ASP.NET Identity.


## Role

A role is a series of permissions of access and/or modification to certain parts of the system and its database applied to registered users.

Roles are controlled by ASP.NET Identity.

This list of roles is in no way complete. New roles can be added to this documentation as needed.


### Uberadmin

**	The uberadmin is the maximum role. It corresponds to the system administrator and gives all permissions, both access and modification, to all parts of the system and its database.**


### Writer

**	The writer is the role that gives an user permission to write new data about an artifact in the database, for example, the history of a company. It can also modify existing data, and approve suggestions of modifications and additions that come from a basic user.**

**	An uberadmin is the only role that can give the *Writer* role.**

***	Modifications of data by the writer are stored in the database as deltas, but do not require approval to be applied to the main tables.**


### Proofreader

***	The proofreader is the role that gives an user permission to correct textual data about an artifact in the database. It is expected to be someone with demonstrated proficiency in the main language of the system (usually American English, *en-US*).**

***	Uberadmin and writer are the only roles that can give the *Proofreader* role.**

***	This role can only modify textual data, not relationships or specifications.**

***	Modifications of textual data by the proofreader are stored in the database as deltas, but do not require approval to be applied to the main tables.**


### Translator

***	The translator is the role that gives an user permission to propose a translation of the textual data about an artifact on the database, or the elements of the system itself. It has assigned one or several languages, different from the main language of the database.**

***	This role can create a new translation, on any of its assigned languages, from any of its assigned languages or from the main language of the database, of any textual data assigned to an artifact or of any text element from the system.**

***	This role can also proofread textual data assigned to artifacts that is in any of its assigned languages.**

***	Modifications created by a translator are hold on approval by a supertranslator role.**

***	Any registered user can ask to become a translator by requesting to translate textual data assigned to an artifact to a language they choose, but not a text element from the system.**

***	The translator role can only be assigned by a uberadmin, or by a supertranslator that has assigned any of the corresponding translator languages.**


### Supertranslator

***	The supertranslator is the role that gives an user permission to create a translation of the textual data about an artifact on the database, or the elements of the system itself. It has assigned one or several languages, different from the main language of the database.**

***	This role can create a new translation, on any of its assigned languages, from any of its assigned languages or from the main language of the database, of any textual data assigned to an artifact or of any text element from the system.**

***	This role can also proofread textual data assigned to artifacts that is in any of its assigned languages.**

***	This role can also approve changes proposed by a translator, that are in any of its assigned languages.**

***	Modifications created by a supertranslator are stored as deltas in the databases and applied to the main tables without approval.**

***	Any translator can be promoted to supertranslator.**

***	A registered user can be a supertranslator in a set of languages, but only a translator in any other set of non-overlapping languages.**

***	A uberadmin can promote any translator to a translator of any language.**

***	A supertranslator can promote any translator as a supertranslator of any language it has assigned.**


### Collaborator

***	The collaborator is the role that gives an user permission to propose adding or modifying an artifact in the database.**

***	This role can create a new artifact of any type in the database (except for inventory and repairs), or modify the data from any existing artifact (except for inventory and repairs), as well as upload any photography or scan of any existing artifact.**

***	Additions, modifications and uploads from a collaborator are hold on approval by a curator role.**

***	Any registered user can ask to become a collaborator by requesting to add an artifact, modify the data about an existing artifact, or upload a photography or scan.**

***	The collaborator role can be assigned by a uberadmin or by a curator.**


### Curator

***	The curator is the role that gives an user permission to propose adding or modifying an artifact in the database.**

***	This role can create a new artifact of any type in the database (except for inventory and repairs), or modify the data from any existing artifact (except for inventory and repairs), as well as upload any photography or scan of any existing artifact.**

***	Additions, modifications and uploads from a collaborator are stored as deltas in the database and applied to the main tables without approval.**

***	The curator role can be assigned by a uberadmin or by a curator.**


### Physical curator

***	The physical curator has the same permissions as a *curator* but its permissions also apply to inventory artifacts.**

***	The physical curator role can be assigned by a uberadmin or by a physical curator.**


### Technician

***	The technician is the role that can modify repairs of artifacts in the inventory.**

***	This role can add photographies to a repair, part exchanges or request parts for replacement.**

***	This role is indented to give the possibility of auditing of the whole repair process for physical inventory in a museum or collection.**

***	The technician role can be assigned by a uberadmin or a supertechnician.**


### Supertechnician

***	The supertechnician is the role that can create, or close as completed, repairs of artifacts in the inventory.**

***	This role must approve repairs and parts requests.**

***	Also this role has the same permissions as the technician role.**

***	The supertechnician role can only be assigned by a uberadmin.**

## User view

***	The user view is the public part of the system.**

***	It is to be composed of several webpages, organized by artifact types, allowing users to explore the artifacts contained in the database.**

***	Pages should be implemented using Razor, themable throught the usage of Bootstrap and customized CSS.**


## Admin view

***	The admin view is the private part of the system.**

***	The different subsections of this view allow registered users with the appropriate role to modify the artifacts in the database.**

***	The subsections of the view that are visible to a user depends on its roles.**

# **Artifacts**

The purpose of this section is to describe the various artifacts that are stored in the systems databases. They have a one to one relation to tables of a database or models of the applications. All text is written in American English (aka *en-US*) and latin script by default.

*The system administrator can choose other language as the primary text language.


*When an Id field is not specified, or any other specified field is marked as a primary key, it is implicit that the artifact requires such a field for storage in the database.


*Field types in *cursive* represent links to another artifact.


## **Book**

**	This artifact represents books about computing.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Title | *String | Untranslated title in the default system writing script. |
| NativeTitle | *String | Title as written in the book cover. |
| Published | *Date | Date of publication. |
| Country | ** | Country of publication. |
| Synopsis | *FullText | Book synopsis, as printed on it. |
| Isbn | *String(13) | International Standard Book Number. |
| Pages | *Short | Number of pages. |
| Edition | *Int | Edition. |
| Previous | ** | Link to previous edition, if applicable. |
| Next | ** | Link to next edition, if applicable. |
| Source | ** | If this book is a derivate (e.g. translation) of another, link to the original. |


## **Companies by book**

**	This artifact links books and document companies.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Company | ** | Link to the document company. |
| Book | ** | Link to the book. |
| Role | ** | Role the document company has in the book. |


## **Companies by document**

**	This artifact links documents and document companies.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Company | ** | Link to the document company. |
| Document | ** | Link to the document. |
| Role | ** | Role the document company has in the document. |


## **Companies by magazine**

**	This artifact links magazines and document companies.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Company | ** | Link to the document company. |
| Magazine | ** | Link to the magazine. |
| Role | ** | Role the document company has in the magazine. |


## **Company**

**	This artifact represents a business entity that created other artifacts belonging to computing history. It can be, but not limited to, a manufacturer, software developer, publisher, etc.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | String | Company legal name, without company type identifier (no Inc., S.A., etc) |
| Founded | Date | Date when company was legally registered |
| Website | Url | URL of the latest known company official website. |
| Twitter | String(45) | Twitter user of the official company Twitter account. |
| Facebook | String(45) | Facebook account number of the official company Facebook account. |
| Sold | Date | Date the company changed status. |
| SoldTo | ** | Link to the company that acquired this company when it changed status. |
| Address | *String(80) | Last known physical address of the company headquarters. |
| City | String(80) | City of the last known physical address of the company headquarters. |
| Province | String(80) | Province or state of the last known physical address of the company headquarters. |
| Postal code | String(25) | Postal or ZIP code of the last known physical address of the company headquarters. |
| Country | ** | Country code where the company is, or was, legally registered. |
| Status | ** | Current status of the company. |
| DocumentCompany | ** | Link to the DocumentCompany artifact that is equivalent to this company. |


## **Company description**

**	This artifact contains a textual description and history, corresponding to a company.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Company | ** | Company this description belongs to. |
| Text | IndexedText | Markdown version of the description. |
| Html | IndexedText | Rendered and cleaned HTML version of the description. |


## **Company logo**

**	This artifact points to a vectorial representation of the company logo.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Company | ** | Company this logo belongs to. |
| Year | Int\[1000,3000\] | Year the company started usign this logo. |
| Guid | Guid | GUID used to generate the server side file containing the logo. |


## **Currency inflation**

**	This artifact lists the known inflations for a currency, allowing to calculate current-day costs given a previously known cost.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Currency | ** | Currency. |
| Year | *Year | Indicates this inflation becomes effective that year, respective with the currency value the previous year. |
| Inflation | *Float | The inflation occurred at the specified year. |


## **Currency pegging**

**	This artifact links pegged currencies, specially when a currency becomes historic and substituted for another, to allow to convert values between them.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Source | ** | Source currency. |
| Destination | ** | Destination currency. |
| Ratio | *Float | Ratio between source and destination currencies. |
| Start | *Date | Date when the pegging became effective. |
| End | *Date | Date when the pegging finished if applicable. |


## **Document**

**	This artifact represents documents about computing.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Title | *String | Untranslated title in the default system writing script. |
| NativeTitle | *String | Title as written in the document cover. |
| Published | *Date | Date of publication. |
| Country | ** | Country of publication. |
| Synopsis | *FullText | Document synopsis. |


## **Document company**

**	This artifact represents a business entity that created, or published, document artifacts belonging to computing history. It is separate from the *Company* artifact because a document company can publish documents (e.g. books, magazines) while not being exclusively dedicated to computing artifacts.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Company legal name, without company type identifier (no Inc., S.A., etc). |
| Company | ** | Identifier of the company artifact that is the same legal entity as this document company. |
| Books | *\[\]* | List of books this document company has published. |
| Documents | *\[\]* | List of documents this document company has published. |
| Magazines | *\[\]* | List of magazines this document company has published. |


## **Document person**

**	This artifact represents a person that has participated in the creation of a document artifact.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of this person. |
| Surname | *String | Surname of this person. |
| Person | ** | Link to the person artifact. |
| Alias | *String | Alias of this person. |
| DisplayName | *String | Name to be displayed for this person. |
| Books | *\[\]* | List of books by this document person. |
| Documents | *\[\]* | List of documents by this document person. |
| Magazines | *\[\]* | List of magazines by this document person. |


## **Document role**

**	This artifact lists all the roles a person or company can have in a document. It is copied from the EPUB specification list of roles, as standard used worldwide by publishers and libraries.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Id | *String(3) | Id of the role as set in the specification. |
| Name | *String | Role name. |
| Enabled | *Bool | Set if the role is usable, unset if it is historical. |


## **Dump**

**	This artifact represents the act of taking physical storage media and creating an image file of it. Several dumps can however represent the same media dumps, as this artifact is the act itself, not its results, and different dumps can achieve the same results.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Dumper | *String | Name or alias of the person that created the dump. |
| Dumper user | ** | User in the system of the person that created the dump. |
| Dumping group | *String | Dumping group the person that created the dump belongs to. Specially important for import of DAT files from dumping groups, like TOSEC, No-Intro, etc. |
| Date | *DateTime | *Date and time when the dump was done. |
| Upload date | *DateTime | Date and time when the dump was uploaded to the system. |
| Dump hardware | *\[\]* | List of hardware used to create the dump. |
| Media | ** | Link to the media that was dumped. |
| Media dump | ** | Link to the results of the dump. |


## **Dump hardware**

**	This artifact represents information about the hardware, and software, used to create a dump.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Manufacturer | *String | Manufacturer of the drive used to read the media, or of the media itself it non-removable. |
| Model | *String | Model of the drive used to read the media, or of the media itself if non-removable. |
| Revision | *String | Revision of the drive used to read the media, or of the media itself if non-removable. |
| Firmware | *String | *Firmware version of the drive used to read the media, or of the media itself if non-removable. |
| Serial | *String | Serial number of the drive used to read the media, or of the media itself if non-removable. |
| Software name | *String | Name of the software used to read the media. |
| Software version | *String | Version of the software used to read the media. |
| Software OS | *String | Normalized name and version of the operating system where the software that read the media was run. |
| Extents | *\[\]* | List of extents. Blocks in the media not present in the extent where not read at all in the dump. |


## **Extent**

**	This artifact represents a read extent, that is, a start and end of a read block.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Start | *Long | First block number of the extent, inclusive. |
| End | *Long | Last block number of the extent, inclusive. |
| Error | *Bool | If set indicates the extent was tried to read but could not be read without errors. |


## **File**

**	This artifact identifies a computer file, in a unique way, allowing to create file lists from media, dumps, etc.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Size | *Long | Size in bytes of the file. |
| Md5 | Binary(16) | MD5 hash of the file contents. |
| Sha1 | Binary(20) | SHA1 hash of the file contents. |
| Sha256 | Binary(32) | SHA2-256 hash of the file contents. |
| Sha3 | Binary(64) | SHA3-512 hash of the file contents. |
| Spamsum | *String | SpamSum fuzzy hash of the file contents. |
| Mime | *String | MIME type of the file. |
| Magic | *String | Identification string as made by libmagic. |
| AccoustId | *String | Accoust ID of the audio contained in the file. |
| Infected | *Bool | If set, the file is infected by, or is, malware. |
| Malware | *String | Name of the malware contained in the file. |
| Hack | *Bool | If set, the file has been hacked, cracked, or its functionality if facilitating the cracking of software. E.g. serial number, key generator, crack patch, etc... |
| HackGroup | *String | Name of the hacking group. |


## **File data stream**

	This artifacts represents the data stream contents of a file. That is, normal data, a fork, an extended attribute, or an alternate data stream.


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the data stream. *‘NULL’* when it is the default data stream. |
| Size | *Long | Size in bytes of the data stream. |
| Contents | ** | Link to the artifact that uniquely identify the contents of this data stream. |


## **Filesystem**

**	This artifact represents a filesystem contained in a media.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | *String | Filesystem type. |
| CreationDate | *DateTime | Date and time when the filesystem was created. |
| ModificationDate | *DateTime | Date and time when the filesystem was last modified. |
| BackupDate | *DateTime | Date and time when the filesystem was last backed up. |
| ClusterSize | *Int | Size, in bytes, of each cluster or block in the filesystem. |
| Clusters | *Long | Number of clusters or blocks in the filesystem. |
| FilesCount | *Long | Number of files, if known, in the filesystem. |
| Bootable | *Bool | If set, the filesystem is known to contain boot code. |
| VolumeSerial | *String | Filesystem volume serial number. |
| VolumeName | *String | Filesystem volume name. |
| FreeClusters | *Long | Number of free culsters or blocks in the filesystem. |
| ExpirationDate | *DateTime | Date and time when the filesystem expires. |
| EffectiveDate | *DateTime | Date and time when the filesystem becomes effective. |
| SystemIdentifier | *String | System identifier. |
| VolumeSetIdentifier | *String | Volume set identifier. |
| PublisherIdentifier | *String | Publisher identifier. |
| DataPreparerIdentifier | *String | Data preparer identifier. |
| ApplicationIdentifier | *String | Application identifier. |
| Files | *\[\]* | Files contained in the filesystem. |


## **Graphical Processing Unit**

    **This artifact represents a chip, or chipset, whose functionality is the generation of text, bidimensional raster images, bidimensional vectorial images, tridimensional images or raytraced images, to be shown by a machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Commercial name of the graphical processing unit. |
| Company | ** | Identifier of the company artifact that manufactured the graphical processing unit. |
| Model | *String | Model number, SKU or equivalent of this graphical processing unit, if applicable. |
| Introduced | *DateTime | Date of public introduction (sale), minimum value for prototypes and NULL for unknown. |
| Package | *String | If graphical processing unit is a single chip, chip package type. |
| Process | *String | If graphical processing unit is a single chip, chip manufacturing process. |
| ProcessNm | *Float | If graphical processing unit is a single chip, size in nanometers of the manufacturing process. |
| DieSize | *Float | If graphical processing unit is a single chip, size in square milimeters of the die surface area. |
| Transistors | *Long | If graphical processing unit is a single chip, number of transistors, if applicable, that comprise it. |
| Resolutions | *\[\]* | List of resolutions the graphical processing unit can generate. |


## **Instruction set**

**	This artifact lists the known instruction sets that can be implemented by a processor.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the instruction set. |


## **Instruction set extension**

**	This artifact lists the known instruction set extensions that can be implemented by a processor.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the instruction set extension. |


## **ISO 3166-1 Numeric**

**	This artifact contains the list of numeric unique identifiers for countries, existing or historic, as defined by the ISO 3166 standard.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Id | Short | ISO 3166-1 Numeric country code |
| Name | String(64) | Country name |


## **ISO 4217**

**	This artifact lists all currencies, existing and historic, as defined in the ISO 4217 standard.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Code | *String(3) | Alphanumeric unique identifier for the currency. |
| Numeric | *Int(3) | Numerical unique identifier for the currency. |
| Minor unit | *Int(1) | Number of decimal points to represent minor units. |
| Name | *String | Name of the currency. |
| Withdrawn | *Date | Date the currency become historic. |


## **ISO 639**

    **This artifact contains the list of unique identifiers for languages, existing or historic, as defined by the ISO 639 standard.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Id | Char(3) | ISO 639-3 language code |
| Part2B | Char(3) | ISO 639-2 Bibliographic language code |
| Part2T | Char(3) | ISO 639-2 Terminological language code |
| Part1 | Char(2) | ISO 639-1 language code |
| Scope | Char(1) | Language code scope |
| Type | Char(1) | Language type |
| ReferenceName | String(150) | Language reference name |
| Comment | String(150) | Comment on any of the other columns |


## **License**

    **This artifact contains the list of software licenses, document licenses, and other copyright authorization licenses.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | String | License name |
| SPDX | String | SPDX license identifier |
| FsfApproved | Bool | Set if license is approved as open-source by the Free Software Foundation |
| OsiApproved | Bool | Set if license is approved as open-source by the Open Source Initiative |
| Link | Url | Link to the entity responsible of the license creation |
| Text | Text(131072) | Full license text |


## **Logical partition**

**	This artifact represents a logical partition inside a media.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Sequence | *Int | Partition number. |
| Name | *String | Partition name, if the partition scheme includes it. |
| Type | *String | Partition type. |
| FirstSector | *Long | First sector of the partition. |
| LastSector | *Long | Last sector of the partition. |
| Size | *Long | Size in bytes of the partition. |
| Description | *String | Partition description, if the partition scheme includes it. |
| Scheme | *String | Name of the partition scheme. |
| Filesystems | *\[\]* | List of known filesystems residing in this partition. |


## **Machine**

***	This artifact represents a turing complete physical machine, typically a computer, videogame console, arcade board, etc.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the machine. |
| Company | ** | Identifier of the company artifact that manufactured the machine. |
| MachineType | ** | Type of machine. |
| Introduced | *DateTime | Date of public introduction (sale), minimum value for prototypes and NULL for unknown. |
| Family | ** | Identifier of the machine family this machine belongs to. |
| Model | *String | Model number, SKU or equivalent of this machine, if applicable. |
| Gpus | *\[\]* | List of graphical processing units that came installed from factory with this machine. |
| Memory | *\[\]* | List of memory that came installed from factory with this machine. |
| Processors | *\[\]* | List of processors that came installed from factory with this machine. |
| Sound | *\[\]* | List of sound synthetizers that came installed from factory with this machine. |
| Storage | *\[\]* | List of storage units that came installed from factory with this machine. |
| Photos | *\[\]* | List of photographies belonging to this machine that have been uploaded to the system. |
| Screens | *\[\]* | List of screens this machine has physically, and inseparably, installed from factory with this machine. |
| Documents | *\[\]* | List of documents where this machine is a main topic. |
| Books | *\[\]* | List of books where this machine is a main topic. |
| Magazines | *\[\]* | List of magazines issues where this machine is a main topic. |


## **Machine family**

**	This artifact represents an aggrupation of machines that are technically the same, side for some configuration, or regional, differences. As an example, the Sega Genesis / Mega Drive family of videogame consoles, or the Apple Powerbook 1400 family of laptop computers.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the machine family. |
| Company | ** | Identifier of the company artifact that manufactured the machine family members. |
| Documents | *\[\]* | List of documents where this machine family is a main topic. |
| Books | *\[\]* | List of books where this machine family is a main topic. |
| Magazines | *\[\]* | List of magazines issues where this machine family is a main topic. |


## **Machine photo**

***	This artifact represents the photographies about a machine that are stored in the system.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Author | *String | Name of the author of the photo. |
| CameraManufacturer | *String | Manufacturer of the camera. |
| CameraModel | *String | Model of the camera. |
| ColorSpace | * | Colorspace of the photo. |
| Comments | *String | User comments for the photo. |
| Contrast | * | Contrast from EXIF. |
| CreationDate | *DateTime | Date and time the photo was taken. |
| DigitalZoomRatio | *Double | Digital zoom ratio. |
| ExifVersion | *String | EXIF version. |
| Exposure | *String | Exposure time. |
| ExposureMethod | * | Exposure mode from EXIF. |
| ExposureProgram | * | Exposure program from EXIF. |
| Flash | * | Flash mode from EXIF. |
| Focal | *Double | Focal number (F-number). |
| FocalLength | *Double | Focal length in mm. |
| FocalLengthEquivalent | *Ushort | Focal length equivalent for 35mm. |
| HorizontalResolution | *Double | Horizontal resolution. |
| IsoRating | *Ushort | ISO rating. |
| Lens | *String | Lens name. |
| LightSource | * | Light source from EXIF. |
| MeteringMode | * | Metering mode from EXIF. |
| ResolutionUnit | * | Unit for horizontal and vertical resolutions. |
| Orientation | * | Orientation. |
| Saturation | * | Saturation from EXIF. |
| SceneCaptureType | * | Scene capture type from EXIF. |
| SensingMethod | * | Sensing method from EXIF. |
| Sharpness | * | Sharpness from EXIF. |
| SoftwareUsed | *String | Software used in the process of this photo. |
| SubjectDistanceRange | * | Subject distance range from EXIF. |
| UploadDate | *DateTime | Date and time when this photo was uploaded to the system. |
| VerticalResolution | *Double | Vertical resolution. |
| WhiteBalance | * | White balance from EXIF. |
| User | *User | User that uploaded this photo. |
| License | *License | License that covers the rights for this photo. |
| Source | *Url | URL were this photo was taken from, if not original. |
| Machine | ** | Machine this photo belongs to. |


## **Magazine**

**	This artifact represents magazines about computing.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Title | *String | Untranslated title in the default system writing script. |
| NativeTitle | *String | Title as written in the magazine cover. |
| Country | ** | Country of publication. |
| Synopsis | *FullText | Magazine synopsis. |
| Issn | *String(8) | International Standard Serial Number. |
| FirstPublication | *Date | Date when the first issue of the magazine was published. |
| Issues | *\[\]* | List of published issues of this magazine. |


## **Magazine issue**

**	This artifact represents issues from magazines about computing.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Magazine | ** | Link to the magazine information |
| Caption | *String | Untranslated caption in the default system writing script. |
| NativeCaption | *String | Original caption in the original writing script. |
| Published | *DateTime | Date and time when this issue was published. |
| ProductCode | *String(18) | EAN or UPC of this magazine issue. |
| Pages | *Short | Number of pages in this magazine. |
| CoverMedia | *\[\]* | Media included with this magazine issue. |


## **Mastering texts by media**

**	This artifact represents text written in the physical media itself by the mastering or manufacturer facility.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | ** | Mastering text type. |
| Text | *String | Mastering text. |
| Side | *Short | Side if applicable. |
| Layer | *Short | Layer if applicable. |
| Media | ** | Media this mastering text belongs to. |


## **Media**

**	This artifact represents media (disks, tapes, cartridges), that compose a software variant.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Title | *String | Title as shown in media. |
| Sequence | *Short | *Sequence number of media. |
| LastSequence | *Short | Last sequence number in media set. |
| Type | ** | Type of media. |
| WriteOffset | *Int | In Compact Disc and derivates, the number of bytes of write offset. |
| Sides | *Short | For optical discs, number of written/stamped sides. |
| Layers | *Short | For optical discs, number of layers per side. |
| Sessions | *Short | For optical discs, the number of recorded/stamped sessions. |
| Tracks | *Short | For optical discs, the number of recorded/stamped tracks. |
| Sectors | *Long | Total number of sectors in media. |
| Size | *Long | Total number of bytes in media. |
| CopyProtection | *String | Copy protection present in media if applicable. |
| PartNumber | *String | Part number or SKU number. |
| SerialNumber | *String | Serial number from manufacturer when different from part number. This is not the software registration serial number. |
| Barcode | *String | Barcode, if present. |
| CatalogueNumber | *String | Publisher catalogue number. |
| TableOfContents | *\[\]* | For optical discs, the disc TOC. |
| Dumps | *\[\]* | Known dumps for this media. |
| Partitions | *\[\]* | Logical partitions on this media. |
| Manufacturer | *String | Media manufacturer. |
| Model | *String | Media model. |
| Revision | *String | Media revision. |
| Firmware | *String | Media firmware version. |
| Physical block size | *Int | Physical block size of media when constant. |
| Logical block size | *Int | Logical block size of media when constant. |
| Variable block sizes | *\[\]* | List of block sizes per extent. |
| Storage interface | ** | If the media is non-removable from the drive (hard disk, flash drive, PCMCIA card, etc), which interface it uses to connect. |


## **Media dump**

	Represents a known dump of a software media.  


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Format | *String | Normalized name of the media dump file format. |
| Image | ** | Link to media dump image. |
| Tracks | *\[\]* | Link to media dump track image(s). |
| Files | *\[\]* | Link to media dump file image(s). |
| Subchannel | ** | *Link to media dump subchannel image. |
| Dump status flags | ** | Flags giving information about the dump status. |


## **Media dump file image**

	Represents the file that contains the data from the dump of a software media, restricted to the data of a single tape file.


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| File sequence | *Long | File number. |
| Partition sequence | *Short | Partition number this file resides in. |
| Filesystems | *\[\]* | List of known filesystems residing in this file. |
| Size | *Long | Size in bytes of the file. |
| Md5 | *Binary(16) | MD5 hash of the file contents. |
| Sha1 | *Binary(20) | SHA1 hash of the file contents. |
| Sha256 | *Binary(32) | SHA2-256 hash of the file contents. |
| Sha3 | *Binary(64) | SHA3-512 hash of the file contents. |
| Spamsum | *String | SpamSum fuzzy hash of the file contents. |


## **Media dump image**

	Represents the file that contains the data from the dump of a software media, or in the case of some specific formats, the descriptor for the tracks in such dump.


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Size | *Long | Size in bytes of the file. |
| Md5 | *Binary(16) | MD5 hash of the file contents. |
| Sha1 | *Binary(20) | SHA1 hash of the file contents. |
| Sha256 | *Binary(32) | SHA2-256 hash of the file contents. |
| Sha3 | *Binary(64) | SHA3-512 hash of the file contents. |
| Spamsum | *String | SpamSum fuzzy hash of the file contents. |
| AccoustId | *String | Accoust ID of the audio contained in the file. |


## **Media dump track image**

Represents the file that contains the data from the dump of a software media, restricted to the data of a single Compact Disc track.


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Track sequence | *Short | Track number. |
| Format | *String | Normalized name of the format. BINARY if Intel byte ordering for audio tracks, YRANIB if Motorola byte ordering. |
| Size | *Long | Size in bytes of the file. |
| Md5 | *Binary(16) | MD5 hash of the file contents. |
| Sha1 | *Binary(20) | SHA1 hash of the file contents. |
| Sha256 | *Binary(32) | SHA2-256 hash of the file contents. |
| Sha3 | *Binary(64) | SHA3-512 hash of the file contents. |
| Spamsum | *String | SpamSum fuzzy hash of the file contents. |
| AccoustId | *String | Accoust ID of the audio contained in the file. |
| Subchannel | ** | *Link to dump subchannel image for this track only. |


## **Media dump subchannel image**

**	Represents the file that contains the data from the dump of a software media, restricted to the subchannel data from a Compact Disc, being it the full disc or a single track.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Track sequence | *Short | Track number. |
| Subchannel | ** | Indicates if P, Q, P to W, R to W, etc. |
| Size | *Long | Size in bytes of the file. |
| Md5 | *Binary(16) | MD5 hash of the file contents. |
| Sha1 | *Binary(20) | SHA1 hash of the file contents. |
| Sha256 | *Binary(32) | SHA2-256 hash of the file contents. |
| Sha3 | *Binary(64) | SHA3-512 hash of the file contents. |
| Spamsum | *String | SpamSum fuzzy hash of the file contents. |


## **Media file**

**	This artifact represents a file as contained in a filesystem.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Path | *String | Path where the file resides. By default uses ‘/’ as path separator excepts in the cases that’s a legal character for filenames in the filesystem. |
| Name | *String | Filename, without path. |
| PathSeparator | *String(1) | If present, indicates which is the path separator character present in path. Should only be present when the path separator is not ‘/’. |
| IsDirectory | *Bool | If set, indicates this file is a directory. It must not contain a *‘NULL’ *data stream. |
| CreationDate | *DateTime | Date and time when the file was created. |
| AccessDate | *DateTime | Date and time when the file was last opened. |
| StatusChangeDate | *DateTime | Date and time when the file metadata was last changed. |
| BackupDate | *DateTime | Date and time when the file was backed up. |
| LastWriteDate | *DateTime | Date and time when the file was last written or appended. |
| Attributes | ** | File attributes. |
| PosixMode | *Short | POSIX permissions mode. ACLs go in data streams. |
| DeviceNumber | *Int | Device number. |
| GroupId | *Long | Identifier of the owner group. |
| UserId | *Long | Identifier of the owner user. |
| Inode | *Long | Unique identifier of the file in the filesystem it resides. |
| Links | *Long | Number of different paths that point to the same file. |
| DataStreams | *\[\]* | Contents of the file, its extended attributes, forks, and alternate data streams. |


## **Media tag dump**

**	This artifact represents binary data that is part of a media outside of the normal user area data part. E.g. PFI, DMI, etc.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | ** | Media tag type. |
| Tag | ** | Information about the contents of the media tag. |


## **Memory**

**	This artifact represents primary storage, a.k.a. memory, present in a machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | ** | Type of memory. |
| Usage | ** | What is the use of the memory in the machine. |
| Size | *Long | Size, in bytes, of the memory. |
| Speed | *Double | Speed, in Hz, of the memory. |


## **Owned machine**

***	This artifact represents a machine as owned by the system owner (museum, preservation society, etc) or registered users.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| AcquisitionDate | *DateTime | Date the machine was acquired. |
| LostDate | *DateTime | Date when the machine was sold, traded, or otherwise lost. |
| Status | ** | Status of the machine. |
| LastStatusDate | *DateTime | Last status check date and time. |
| Trade | *Bool | If set, the machine is available for trading with other users. |
| Boxed | *Bool | If set, the user has the original boxing materials. |
| Manuals | *Bool | If set, the user has the original manuals. |
| SerialNumber | *String | Serial number of the machine. |
| SerialNumberVisible | *Bool | If set, the serial number of the machine is visible to other users. |
| Machine | ** | Link to machine specifications. |
| User | ** | Link to owner user artifact. |
| Gpus | *\[\]* | List of graphical processing units that are installed in this machine. |
| Memory | *\[\]* | List of memory that are installed in this machine. |
| Processors | *\[\]* | List of processors that are installed in this machine. |
| Sound | *\[\]* | List of sound synthetizers that are installed in this machine. |
| Storage | *\[\]* | List of storage units that are installed in this machine. |
| Photos | *\[\]* | List of photographies belonging to this machine that have been uploaded to the system. |


## **Owned machine photo**

***	This artifact represents the photographies about an owned machine that are stored in the system.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Author | *String | Name of the author of the photo. |
| CameraManufacturer | *String | Manufacturer of the camera. |
| CameraModel | *String | Model of the camera. |
| ColorSpace | * | Colorspace of the photo. |
| Comments | *String | User comments for the photo. |
| Contrast | * | Contrast from EXIF. |
| CreationDate | *DateTime | Date and time the photo was taken. |
| DigitalZoomRatio | *Double | Digital zoom ratio. |
| ExifVersion | *String | EXIF version. |
| Exposure | *String | Exposure time. |
| ExposureMethod | * | Exposure mode from EXIF. |
| ExposureProgram | * | Exposure program from EXIF. |
| Flash | * | Flash mode from EXIF. |
| Focal | *Double | Focal number (F-number). |
| FocalLength | *Double | Focal length in mm. |
| FocalLengthEquivalent | *Ushort | Focal length equivalent for 35mm. |
| HorizontalResolution | *Double | Horizontal resolution. |
| IsoRating | *Ushort | ISO rating. |
| Lens | *String | Lens name. |
| LightSource | * | Light source from EXIF. |
| MeteringMode | * | Metering mode from EXIF. |
| ResolutionUnit | * | Unit for horizontal and vertical resolutions. |
| Orientation | * | Orientation. |
| Saturation | * | Saturation from EXIF. |
| SceneCaptureType | * | Scene capture type from EXIF. |
| SensingMethod | * | Sensing method from EXIF. |
| Sharpness | * | Sharpness from EXIF. |
| SoftwareUsed | *String | Software used in the process of this photo. |
| SubjectDistanceRange | * | Subject distance range from EXIF. |
| UploadDate | *DateTime | Date and time when this photo was uploaded to the system. |
| VerticalResolution | *Double | Vertical resolution. |
| WhiteBalance | * | White balance from EXIF. |
| User | * | User that uploaded this photo. |
| License | * | License that covers the rights for this photo. |
| OwnedMachine | ** | Owned machine this photo belongs to. |


## **People by book**

**	This artifact links books and document people.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Person | ** | Link to the document person. |
| Book | ** | Link to the book. |
| Role | ** | Role the document person has in the book. |


## **People by document**

**	This artifact links documents and document people.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Person | ** | Link to the document person. |
| Document | ** | Link to the document. |
| Role | ** | Role the document person has in the document. |


## **People by magazine**

**	This artifact links magazines and document people.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Person | ** | Link to the document person. |
| Magazine | ** | Link to the magazine. |
| Role | ** | Role the document person has in the magazine. |


## **People**

***	This artifact stores information about people that has been important in the computing history.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Person name. |
| Surname | *String | Person surname. |
| ContryOfBirth | ** | Person country of birth (use historic, if applicable). |
| BirthDate | *Date | Date of birth. |
| DeathDate | *Date | Date of death if applicable. |
| Webpage | *Url | Official webpage. |
| Twitter | *String | Official Twitter handle. |
| Facebook | *String | Official Facebook handle. |
| Photo | *Guid | GUID that uniquely identifies the photo of this person that is stored in the system. |
| DocumentPerson | ** | Link to document person artifact. |
| Alias | *String | Alias name for the person. |
| DisplayName | *String | Name to be displayed for the person. |


## **Processor**

**	This artifact represents the chip, or chipset, that processes the data, inputs and outputs, or basically, runs the turing machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Commercial name of the processor. |
| Company | ** | Identifier of the company artifact that manufactured the processor. |
| Model | *String | Model number, SKU or equivalent of this processor, if applicable. |
| Introduced | *DateTime | Date of public introduction (sale), minimum value for prototypes and NULL for unknown. |
| Speed | *Double | Nominal sustained speed of the processor. This field can change in the link with a *machine*. |
| Package | *String | If processor is a single chip, chip package type. |
| Gprs | *Int | Number of general purpose registers, that is, registers used for storage and calculations with integer numbers. |
| GprSize | *Int | Size, in bits, of the general purpose registers. |
| Fprs | *Int | Number of floating point registers, that is, registers used for storage and calculations with floating point numbers. |
| FprSize | *Int | Size, in bits, of the floating point registers. If this value is positive, but the number of FPRs is 0, means floating point numbers of the specified bits are stored in the general purporse registers. |
| Cores | *Int | Number of simultaneous executing complete cores. |
| ThreadsPerCore | *Int | Number of simultaneous threads each core can execute. |
| Process | *String | If processor is a single chip, chip manufacturing process. |
| ProcessNm | *Float | If processor is a single chip, size in nanometers of the manufacturing process. |
| DieSize | *Float | If processor is a single chip, size in square milimeters of the die surface area. |
| Transistors | *Long | If processor is a single chip, number of transistors, if applicable, that comprise it. |
| DataBus | *Int | Size in bits of the external data bus. |
| AddrBus | *Int | Size in bits of the external address bus. |
| SimdRegisters | *Int | Number of registers designated for SIMD calculations. |
| SimdSize | *Int | Size of registers designated for SIMD calculations. If this has a positive value, but the number of registers is zero, it means SIMD calculations are executed in the FPRs (or the GPRs if they are zero). |
| L1Instruction | *Float | Size in kibibytes of the L1 instruction cache. |
| L1Data | *Float | Size in kibibytes of the L1 data cache. |
| L2 | *Float | Size in kibibytes of the L2 cache. |
| L3 | *Float | Size in kibibytes of the L3 cache. |
| InstructionSet | ** | Instruction set implemented by this processor. |
| InstructionSetExtensions | *\[\]* | List of extensions of the instruction set implemented by this processor. |


## **Processor by machine**

**	This artifact links a processor artifact and a machine artifact.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Processor | ** | Link to processor. |
| Machine | ** | Link to machine. |
| Speed | *Float | Speed, in MHz, the linked processor runs at in the linked machine. |


## **Processor by owned machine**

**	This artifact links a processor artifact and an owned machine artifact.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Processor | ** | Link to processor. |
| OwnedMachine | ** | Link to owned machine. |
| Speed | *Float | Speed, in MHz, the linked processor runs at in the linked owned machine. |


## **Resolution**

**	This artifact represents a graphical resolution characteristics, to be generated by a graphical processing unit.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Width | *Int | Width of the resolution. |
| Height | *Int | Height of the resolution. |
| Colors | *Long | Number of simultaneous colors in the resolution. |
| Palette | *Long | Number of colors available non simultaneously in the resolution. |
| Chars | *Bool | If set, width and height indicate text characters. If not, they indicate pixels. |
| Grayscale | *Bool | If set, colors and palette refer to number of shades of gray. |


## **Screen**

**	This artifact represents a physical screen.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Width | *Double | Width of the visible area of the screen in mm. |
| Height | *Double | Height of the visible area of the screen in mm. |
| Diagonal | *Double | Diagonal of the visible area of the screen in inches. |
| NativeResolution | ** | Native resolution of the screen. |
| EffectiveColors | *Long | Maximum number of effective colors the screen can show. |
| Type | *String | Screen physical type. |
| Resolutions | *\[\]* | List of resolutions supported by the screen. |


## **Software family**

**	This artifact identifies software, and englobes all different platforms, regional and language, or other variations of the software, that are finally published, distributed, or otherwise prototyped.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the software family. |
| Introduction | *Date | Date the software was first distributed in any way, or special values for never published prototypes. |
| Companies | *CompaniesBySoftware\[\]* | Companies that participated in the creation and/or distribution of the software. |
| People | *PeopleBySoftware\[\]* | People that participated in the creation and/or distribution of the software. |
| Versions | *\[\]* | Versions of this software. |
| Parent | *\[\]* | Software family this software family is a direct derivate, or continuation from. |


## **Software variant**

**	This artifact represents a specific distributed variant of a software version. A variant can be a different language, an upgrade only variant, a port to another platform, etc.**



| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the software variant. |
| Version | *String | Software version number, if different from the parent *SoftwareVersion.* |
| Introduction | *Date | Date this variant of the software was first distributed in any way, or special values for never published prototypes. |
| Companies | *CompaniesBySoftware\[\]* | Companies that participated in the creation and/or distribution of this variant of the software. |
| People | *PeopleBySoftware\[\]* | People that participated in the creation and/or distribution of this variant of the software. |
| Parent | ** | Software variant this variant is a direct derivate from. |
| Architectures | *\[\]* | Instruction set architectures this software variant runs on. |
| Languages | *\[\]* | Languages this variant runs on. |
| RequiredProcessors | *\[\]* | List of minimum required processor to run this variant. |
| RequiredGpus | *\[\]* | List of minimum required graphical processing units to run this variant. |
| RequiredSoundSynths | *\[\]* | List of sound synthetizers supported by this variant. |
| RequiredMemory | *Long | Minimum required available primary memory for running this variant, in bytes. |
| RecommendedMemory | *Long | Recommended available primary memory for running this variant, in bytes |
| RequiredStorage | *Long | Minimum required available secondary storage, not including the installation media, for running this variant, in bytes. |
| RequiredOperatingSystem | *\[\]* | Minimum required operating system versions this software variant requires to run. |
| RequiredSoftware | *\[\]* | Software versiosn that this variant requires to install, or run. This field shall not be an operating system except when this variant is another operating system and the old one will be upgraded to this version/variant. |
| MachineFamilies | *\[\]* | List of machines families this software variant runs on. |
| Machines | *\[\]* | List of machines this family is restricted to run on. E.g. operating system versions that are locked to install on specific machines only. |
| Files | *\[\]* | Files that make the installer, when distribution is purely digital (but not a disk image), or network install. |
| Media | *\[\]* | Media that makes the software variant, or when distribution is a digital disk image. |
| PartNumber | *String | Part number or SKU number. |
| SerialNumber | *String | Serial number from manufacturer when different from part number. This is not the software registration serial number. |
| ProductCode | *String | Product code, as present in barcode, if applicable. |
| CatalogueNumber | *String | Publisher catalogue number. |
| DistributionMode | ** | How is the software variant distributed. |


## **Software version**

**	This artifact identifies a specific software version.**



| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Name of the software version. |
| Codename | *String | Software codename. |
| Version | *String | Software version number*.* |
| Introduction | *Date | Date this version of the software was first distributed in any way, or special values for never published prototypes. |
| License | ** | License of this version of the software. |
| Companies | *CompaniesBySoftware\[\]* | Companies that participated in the creation and/or distribution of this version of the software. |
| People | *PeopleBySoftware\[\]* | People that participated in the creation and/or distribution of this version of the software. |
| Previous | ** | Next known version of this software. |
| Next | ** | Previous known version of this software. |
| Variants | *\[\]* | Variants of this software version. |


## **Sound synthetizer**

**	This artifact represents a chip, or chipset, whose functionality is the generation of sounds from a machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Name | *String | Commercial name of the processor. |
| Company | ** | Identifier of the company artifact that manufactured the sound synthetizer. |
| ModelCode | *String | Model number, SKU or equivalent of this sound synthetizer, if applicable. |
| Introduced | *DateTime | Date of public introduction (sale), minimum value for prototypes and NULL for unknown. |
| Voices | *Int | Number of PCM voices the sound synthetizer can generate simultaneously. |
| Frequency | *Double | Sample rate in Hz. |
| Depth | *Int | Sample resolution in bits. |
| SquareWave | *Int | Number of square wave generators in the sound synthetizer. |
| WhiteNoise | *Int | Number of white noise generators in the sound synthetizer. |
| Type | ** | Type of sound synthetizer. |


## **Standalone files**

**	This artifact represents the file, or set of files, used to install software, when it does not come as a media. It has exactly the same fields as *.***


## **Storage by machine**

**	This artifact represents a secondary storage, usually a disk drive, installed on a machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | ** | Storage type. |
| Interface | ** | Interface the storage uses to connect to the machine. |
| Capacity | *Long | Capacity, in bytes, of the storage. |


## **Storage by owned machine**

**	This artifact represents a secondary storage, usually a disk drive, installed on a owned machine.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Type | ** | Storage type. |
| Interface | ** | Interface the storage uses to connect to the machine. |
| Capacity | *Long | Capacity, in bytes, of the storage. |

## **Software variant by compilation**

**	This artifact represents the software that is included in compilation media, such as magazine cover media.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Path | *String | Relative path, where the software resides inside the magazine media. NULL if the whole media belongs to the software. |
| PathSeparator | *String(1) | If present, indicates which is the path separator character present in path. Should only be present when the path separator is not ‘/’. |
| Software | ** | Link to the software variant. |
| Media | ** | *Link to the media included as cover in the magazine issue. |


## **Table of contents**

**	This artifact represents the table of contents of an optical disc.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| TrackNumber | *Int | Track number. |
| SessionNumber | *Int | Session number. |
| FirstSector | *Long | First sector of the track, inclusive of any pregap. |
| LastSector | *Long | Last sector of the track, inclusive of any postgap. |
| TrackType | ** | Track type. |


## **Variable block size**

**	This artifact represents an extent of media that has a constant block size, but can be different from another extent in the same media.**


| **Field name** | **Field type** | **Description** |
| :-: | :-: | :-: |
| Start | *Long | First sector, inclusive, of this extent. |
| End | *Long | Last sector, inclusive, of this extent. |
| Size | *Long | Size in bytes of the blocks contained in this extent. |




# **Enumerations**

The purpose of this section is to describe the various enumerations that help to classify the artifacts


## **Status type**

**	This enumeration lists the status an owned machine can have.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | The status of the owned machine is unknown. |
| TestedGood | The last time the owned machine was tested it was working correctly. |
| NotTested | The owned machine has never been tested. |
| TestedBad | The last time the owned machine was tested it presented several problems. |


## **Company status**

**	This enumeration lists the status of a company.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | The status of the company is unknown or not set. |
| Active | The company exists and is active. |
| Sold | The company was sold, totally or partially. |
| Merged | The company merged with another to form a third company. |
| Bankrupt | The company legally filled for bankruptcy. |
| Defunct | The company ceased operations for reasons different to bankruptcy. |
| Renamed | The company was renamed, possibly with a change of intentions. |


## **Machine type**

**	This enumeration lists the types of machines.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | Unknown machine type. |
| Computer | Computer. |
| Console | Videogame console. |
| Arcade | Arcade board. Can be a single game board, or a multiple game board. |
| Pda | Personal digital assistant. Also modern tablets fall in this category. |
| Smartphone | Personal digital assistant with integrated phone capabilities. |


## **Memory type**

**	This enumeration lists the types of primary storage.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | The type of the memory is unknown or not set. |
| DRAM | Dynamic RAM |
| FPM | Fast page mode DRAM |
| EDO | Extended Data Out DRAM |
| VRAM | Dual-ported video DRAM |
| SDRAM | Synchronous DRAM |
| DDR | DDR SDRAM |
| DDR2 | DDR SDRAM v2 |
| DDR3 | DDR SDRAM v3 |
| DDR4 | DDR SDRAM v4 |
| DDR5 | DDR SDRAM v5 |
| RDRAM | Rambus DRAM |
| SGRAM | Synchronous Graphics RAM |
| PSRAM | Pseudostatic RAM |
| SRAM | Static RAM |
| ROM | Read-only memory |
| PROM | Programmable ROM |
| EPROM | Erasable and programmable ROM |
| EEPROM | Electronically-erasable and programmable ROM |
| NAND | NAND flash |
| NOR | NOR flash |
| ReRAM | Resistive RAM |
| CBRAM | Conductive-bridging RAM |
| DWM | Domain-wall memory |
| NanoRAM | Nano-RAM |
| Millipede | Millipede memory |
| FJG | Floating Junction Gate RAM |
| PunchedPaper | Punched paper |
| DrumMemory | Drum memory |
| MagneticCore | Magnetic-core |
| PlatedWire | Plated wire memory |
| CoreRope | Core rope memory |
| ThinFilm | Thin-film memory |
| Twistor | Twistor memory |
| Bubble | Bubble memory |


## **Memory usage**

**	This enumeration lists the kind of uses for primary storage.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | The use of the memory is unknown or not set. |
| Bootloader | Contains a boot loader (usually read-only) whose only function is to load the next memory (firmware or cartridge). |
| Firmware | Contains hardware initialization code, some (or many) low level calls and code to load software from secondary storage. |
| Work | Memory used by software running in the machine. |
| Video | Memory used by the graphics processing units. |
| Sound | Memory used by the sound synthetizers. |
| Wavetable | Memory used to store wave tables. |
| StorageBuffer | Memory used as a buffer from secondary storage. |
| Save | Memory used to save arbitrary data and possibly also configuration. |
| Configuration | Memory used to save only configuration. |
| Unified | Memory accessible directly to any of the processors in the machine, including graphical processing units and sound synthetizers. |


## **Storage type**

**	This enumeration lists the types of secondary storage.**


| **Value** | **Description** |
| :-: | :-: |
| Empty | The interface is empty. |
| Unknown | The type of storage is unknown or not set. |
| MagnetoOptical | Unknown type of magneto-optical drive. |
| HardDisk | Generic hard disk drive. |
| Microdrive | IBM/Hitachi Microdrive |
| ZonedHardDisk | Generic zoned hard disk drive. |
| FlashDrive | Generic flash drive. |
| CompactDisc | Compact Disc. |
| DDCD | Double-Density Compact Disc. |
| PD650 | PD650 |
| DVD | DVD |
| DVDRAM | DVD-RAM |
| HDDVD | HD DVD |
| Bluray | Blu-ray |
| EVD | Enhanced Versatile Disc |
| FVD | Forward Versatile Disc |
| HVD | Holographic Versatile Disc |
| CBHD | China Blue High Definition |
| HDVMD | High Definition Versatile Multilayer Disc |
| VCDHD | Versatile Compact Disc High Density |
| SVOD | Stacked Volumetric Optical Disc |
| FDDVD | Five Dimensional disc |
| LD | LaserDisc |
| LDROM | LaserDisc with digital data (Pioneer variation) |
| LDROM2 | LaserDisc for LD-ROM2 PAC |
| LVROM | LaserVision with digital data (Philips variation) |
| MegaLD | LaserDisc for Mega LD PAC |
| MD | MiniDisc |
| MDData | MiniDisc Data |
| MDData2 | MiniDisc Data 2 |
| HiMD | HiMD |
| UDO | UDO |
| UDO2 | UDO 2 |
| PlayStationMemoryCard | PlayStation memory card |
| PlayStationMemoryCard2 | PlayStation 2 memory card |
| PS1CD | PlayStation Compact Disc |
| PS2CD | PlayStation 2 Compact Disc |
| PS2DVD | PlayStation 2 DVD |
| PS3DVD | PlayStation 3 DVD |
| PS3BD | PlayStation 3 Blu-ray |
| PS4BD | PlayStation 4 Blu-ray |
| UMD | PlayStation Portable Universal Media Disc |
| XGD | Xbox Game Disc |
| XGD2 | Xbox 360 Game Disc |
| XGD3 | Xbox 360 Game Disc (later variant) |
| XGD4 | Xbox One Game Disc |
| MEGACD | Sega MegaCD (Sega CD) Compact Disc |
| SATURNCD | Sega Saturn Compact Disc |
| GDROM | Sega/Yamaha GD-ROM |
| SegaCard | Sega game card |
| HuCard | Hudson Soft game card |
| SuperCDROM2 | Super CD-ROM2 |
| JaguarCD | Jaguar Compact Disc |
| ThreeDO | 3DO Compact Disc |
| PCFX | NEC PC-FX Compact Disc |
| NeoGeoCD | SNK Neo-Geo Compact Disc |
| Floppy | 8” floppy |
| Minifloppy | 5¼” floppy |
| Microfloppy | 3½” floppy |
| AppleFileWare | Apple FileWare floppy |
| Bernoulli | Iomega Bernoulli |
| Bernoulli2 | Iomega Bernoulli (2nd generation) |
| Ditto | Iomega Ditto |
| DittoMax | Iomega Ditto MAX |
| Jaz | Iomega JAZ |
| Jaz2 | Iomega JAZ (2nd generation) |
| PocketZip | Iomega Clik! (aka PocketZip) |
| REV120 | Iomega REV (120Gb variant) |
| REV70 | Iomega REV (70Gb variant) |
| REV35 | Iomega REV (35Gb variant) |
| ZIP100 | Iomega ZIP (100Mb variant) |
| ZIP250 | Iomega ZIP (250Mb variant) |
| ZIP750 | Iomega ZIP (750Mb variant) |
| CompactCassette | Philips Compact Cassette |
| Data8 | Data8 cassette |
| MiniDV | MiniDV cassette |
| CFast | CFast memory card |
| CompactFlash | CompactFlash memory card |
| CompactFlashType2 | CompactFlash memory card (type 2) |
| EZ135 | Syquest EZ135 |
| EZ230 | Syquest EZ230 |
| Quest | Syquest Quest |
| SparQ | Syquest SparQ |
| SQ100 | Syquest SQ100 |
| SQ200 | Syquest SQ200 |
| SQ300 | Syquest SQ300 |
| SQ310 | Syquest SQ310 |
| SQ327 | Syquest SQ327 |
| SQ400 | Syquest SQ400 |
| SQ800 | Syquest SQ800 |
| SQ1500 | Syquest SQ1500 |
| SQ2000 | Syquest SQ2000 |
| SyJet | Syquest SyJet |
| FamicomGamePak | Nintendo Famicom Game Pak |
| GameBoyAdvanceGamePak | Nintendo Game Boy Advance Game Pak |
| GameBoyGamePak | Nintendo Game Boy Game Pak |
| GOD | Nintendo Gamecube Optical Disc |
| N64DD | Nintendo 64 Disk Drive |
| N64GamePak | Nintendo 64 Game Pak |
| NESGamePak | Nintendo Entertainment System Game Pak |
| Nintendo3DSGameCard | Nintendo 3DS Game Card |
| NintendoDiskCard | Nintendo Famicom Disk Card |
| NintendoDSGameCard | Nintendo DS Game Card |
| NintendoDSiGameCard | Nintendo DSi Game Card |
| SNESGamePak | Super Nintendo Entertainment System Game Pak and Super Famicom Game Pak |
| SNESGamePakUS | Super Nintendo Entertainment System Game Pak (USA variant) |
| WOD | Nintendo Wii Optical Disc |
| WUOD | Nintendo Wii U Optical Disc |
| SwitchGameCard | Nintendo Switch Game Card |
| MemoryStick | Memory Stick |
| MemoryStickDuo | Memory Stick Duo |
| MemoryStickMicro | Memory Stick Micro |
| MemoryStickPro | Memory Stick Pro |
| MemoryStickProDuo | Memory Stick Pro Duo |
| microSD | microSD memory card |
| miniSD | miniSD memory card |
| SecureDigital | SecureDigital memory card |
| MMC | MultiMediaCard memory card |
| MMCmicro | MMCmicro memory card |
| RSMMC | RS-MMC memory card |
| MMCplus | MMCplus memory card |
| MMCmobile | MMCmobile memory card |
| eMMC | eMMC memory card |
| MO120 | Generic 120mm magneto-optical |
| MO90 | Generic 90mm magneto-optical |
| MO300 | Generic 300mm magneto-optical |
| MO356 | Generic 356mm magneto-optical |
| CompactFloppy | 3” floppy |
| DemiDiskette | 4” floppy |
| Floptical | 3½” floppy with optical technology by Insite |
| HiFD | 3½” floppy with optical technology by Sony |
| QuickDisk | 2.8” floppy |
| UHD144 | 3½” floppy with optical technology by Caleb |
| VideoFloppy | 2” floppy with analogue video |
| Wafer | Rotronics Wafadrive |
| ZXMicrodrive | Sinclair ZX Microdrive |
| BeeCard | BeeCard |
| Borsu | Borsu |
| DataStore | DataStore |
| MiniCard | MiniCard |
| Orb | Castlewood Orb |
| Orb5 | Castlewood Orb (2nd generation) |
| SmartMedia | SmartMedia memory card |
| xD | xD memory card |
| XQD | XQD memory card |
| DataPlay | DataPlay |
| LS120 | 3½” floppy with optical technology by Imation |
| LS240 | 3½” floppy with optical technology by Imation (2nd generation) |
| FD32MB | 3½” standard floppy formatted with optical technology by Imation |
| RDX | RDX interchangable disk interface |
| PunchedCard | Punched paper |


## **Storage interface**

**	This enumeration lists the interfaces a machine can provide for connection of secondary storage.**


| **Value** | **Description** |
| :-: | :-: |
| Unknown | The interface for storage is unknown or not set. |
| ACSI | Atari Computer System Interface. |
| ATA | AT attachment. |
| XTA | XT attachment. |
| ESDI | Enhanced Small Disk Interface. |
| SCSI | Small Computer System Interface. |
| USB | Universal Serial Bus. |
| FireWire | FireWire. |
| SASI | Shugart Associates System Interface. |
| ST506 | Seagate ST-506 interface. |
| IPI | Intelligent Peripheral Interface. |
| SMD | Storage Module Device. |
| SATA | Serial ATA. |
| SSA | Serial Storage Architecture. |
| DSSI | Digital Storage Systems Interconnect. |
| HIPPI | High Performance Parallel Interface. |
| SAS | Serial Attached SCSI. |
| FC | Fibre Channel. |
| PCIe | PCI Express. |
| M2 | M.2 |
| SataExpress | SATA Express. |


## **ColorSpace**

**	This enumeration lists the photo color spaces defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 1 | sRGB |
| 2 | Adobe RGB |
| 4093 | Wide Gamut RGB |
| 65534 | ICC Profile |
| 65535 | Uncalibrated |


## **Contrast**

**	This enumeration lists the photo contrasts defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal |
| 1 | Low |
| 2 | High |


## **ExposureMode**

**	This enumeration lists the photo exposure modes defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Auto |
| 1 | Manual |
| 2 | Auto bracket |


## **ExposureProgram**

**	This enumeration lists the photo exposure programs defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Not defined |
| 1 | Manual |
| 2 | Program AE |
| 3 | Aperture-priority AE |
| 4 | Shutter speed priority AE |
| 5 | Creative (slow speed) |
| 6 | Action (high speed) |
| 7 | Portrait |
| 8 | Landscape |
| 9 | Bulb |


## **FlashMode**

**	This enumeration lists the photo flash modes defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | No flash. |
| 1 | Fired. |
| 5 | Fired, return not detected. |
| 7 | Fired, return detected. |
| 8 | On, did not fire. |
| 9 | On, fired. |
| 13 | On, return not detected. |
| 15 | On, return detected. |
| 16 | Off, did not fire. |
| 20 | Off, did not fire, return not detected. |
| 24 | Auto, did not fire. |
| 25 | Auto, fired. |
| 29 | Auto, fired, return not detected. |
| 31 | Auto, fired, return detected. |
| 32 | No flash function. |
| 48 | Off, no flash function. |
| 65 | Fired, red-eye reduction. |
| 69 | Fired, red-eye reduction, return not detected. |
| 71 | Fired, red-eye reduction, return detected. |
| 73 | On, red-eye reduction. |
| 77 | On, red-eye reduction, return not detected. |
| 79 | On, red-eye reduction, return detected. |
| 80 | Off, red-eye reduction. |
| 88 | Auto, did not fire, red-eye reduction. |
| 89 | Auto, fired, red-eye reduction. |
| 93 | Auto, fired, red-eye reduction, return not detected. |
| 95 | Auto, fired, red-eye reduction, return detected. |


## **LightSource**

**	This enumeration lists the photo light sources defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Unknown. |
| 1 | Daylight. |
| 2 | Fluorescent. |
| 3 | Tungsten (incandescent). |
| 4 | Flash. |
| 9 | Fine weather. |
| 10 | Cloudy. |
| 11 | Shade. |
| 12 | Daylight fluorescent. |
| 13 | Day white fluorescent. |
| 14 | Cool white fluorescent. |
| 15 | White fluorescent. |
| 16 | Warm white fluorescent. |
| 17 | Standard light A. |
| 18 | Standard light B. |
| 19 | Standard light C. |
| 20 | D55. |
| 21 | D65. |
| 22 | D75. |
| 23 | D50. |
| 24 | ISO Studio Tungsten. |
| 255 | Other. |


## **MeteringMode**

**	This enumeration lists the photo metering modes defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Unknown. |
| 1 | Average. |
| 2 | Center-weighted average. |
| 3 | Spot. |
| 4 | Multi-spot. |
| 5 | Multi-segment. |
| 6 | Partial. |
| 255 | Other. |


## **Orientation**

**	This enumeration lists the photo orientations defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 1 | Horizontal. |
| 2 | Mirror horizontal. |
| 3 | Rotate 180***°**. |
| 4 | Mirror vertical. |
| 5 | Mirror horizontal and rotate 270***°** clock-wise. |
| 6 | Rotate 90***° clock-wise.** |
| 7 | Mirror horizontal and rotate 90***° clock-wise.** |
| 8 | Rotate 270***° clock-wise.** |


## **ResolutionUnit**

**	This enumeration lists the photo resolution units defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 1 | None. |
| 2 | Inches. |
| 3 | ***Centimeters**. |


## **Saturation**

**	This enumeration lists the photo saturations defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


## **SceneCaptureType**

**	This enumeration lists the photo scene capture types defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Standard. |
| 1 | Landscape. |
| 2 | ***Portrait**. |
| 3 | Night. |


## **SensingMethod**

**	This enumeration lists the photo sensing methods defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 1 | Not defined. |
| 2 | One-chip color area. |
| 3 | Two-chip color area. |
| 4 | Three-chip color area. |
| 5 | Color sequential area. |
| 7 | Trilinear. |
| 8 | Color sequential linear. |


## **SubjectDistanceRange**

**	This enumeration lists the photo subject distance ranges defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Unknown. |
| 1 | Macro. |
| 2 | ***Close**. |
| 3 | Distant. |


## **WhiteBalance**

**	This enumeration lists the photo white balances defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Auto. |
| 1 | Manual. |


## **Sharpness**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


## **Mastering text type**

**	This enumeration lists the types of mastering text.**


| **Value** | **Description** |
| :-: | :-: |
| LotNo | Lot number, usuallly printed or engraved in floppies. |
| MastSID | Mastering SID code. |
| MouldSID | Mould SID code. |
| MastCode | Mastering code, written by laser. |
| Barcode | Barcode (not BCA) of optical discs, usually Code 39. |
| Toolstamp | Toolstamp code, engraved. |


## **Media type**

**	This enumeration lists the type of media. It is maintained in sync with Aaru media type enumeration.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Unknown. |
| 1 | Unknown magneto-optical |
| 2 | Generic hard disk. |
| 3 | Microdrive type hard disk. |
| 4 | Zone hard disk. |
| 5 | USB flash drive. |
| 6 | Unknown streamed block tape. |
| 10 | Unknown or non-standard Compact Disc |
| 11 | Compact Disc Digital Audio (Red Book) |
| 12 | CD+G (Red Book) |
| 13 | CD+EG (Red Book) |
| 14 | CD-i (Green Book) |
| 15 | CD-ROM (Yellow Book) |
| 16 | CD-ROM XA (Yellow Book) |
| 17 | CD+ (Blue Book) |
| 18 | CD-MO (Orange Book) |
| 19 | CD-Recordable (Orange Book) |
| 20 | CD-ReWritable (Orange Book) |
| 21 | Mount-Rainier CD-RW |
| 22 | Video CD (White Book) |
| 23 | Super Video CD (White Book) |
| 24 | Photo CD (Beige Book) |
| 25 | Super Audio CD (Scarlet Book) |
| 26 | Double-Density CD-ROM (Purple Book) |
| 27 | DD CD-R (Purple Book) |
| 28 | DD CD-RW (Purple Book) |
| 29 | DTS audio CD (non-standard) |
| 30 | CD-MIDI (Red Book) |
| 31 | CD-Video (ISO/IEC 61104) |
| 32 | PD650 (ECMA-240, ISO 15485) |
| 33 | PD650 WORM (ECMA-240, ISO 15485) |
| 34 | CD-i Ready |
| 35 | FM-Towns disc |
| 40 | DVD-ROM (applies to DVD Video and DVD Audio) |
| 41 | DVD-R |
| 42 | DVD-RW |
| 43 | DVD+R |
| 44 | DVD+RW |
| 45 | DVD+RW DL |
| 46 | DVD-R DL |
| 47 | DVD+R DL |
| 48 | DVD-RAM |
| 49 | DVD-RW DL |
| 50 | DVD-Download |
| 51 | HD DVD-ROM (applies also to HD DVD Video) |
| 52 | HD DVD-RAM |
| 53 | HD DVD-R |
| 54 | HD DVD-RW |
| 55 | HD DVD-R DL |
| 56 | HD DVD-RW DL |
| 60 | BD-ROM (applies also to Blu-ray video) |
| 61 | BD-R |
| 62 | BD-RE |
| 63 | BD-R XL |
| 64 | BD-RE XL |
| 70 | Enhanced Versatile Disc |
| 71 | Forward Versatile Disc |
| 72 | Holographic Versatile Disc |
| 73 | China Blue High Definition |
| 74 | High Definition Versatile Multilayer Disc |
| 75 | Versatile Compact Disc High Density |
| 76 | Stacked Volumetric Optical Disc |
| 77 | Five Dimensional disc |
| 80 | LaserDisc or LaserVision |
| 81 | Pioneer LD-ROM |
| 82 | Pioneer LD-ROM2 |
| 83 | Philips LV-ROM |
| 84 | Pioneer MegaLD |
| 90 | Sony Hi-MD |
| 91 | Sony MiniDisc |
| 92 | Sony MiniDisc for Data |
| 93 | Sony MiniDisc for Data 2 |
| 100 | UDO (ECMA-350, ISO 17345) |
| 101 | UDO2 (ECMA-380, ISO 11976) |
| 102 | UDO2 WORM (ECMA-380, ISO 11976) |
| 110 | PlayStation Memory Card |
| 111 | PlayStation 2 Memory Card |
| 112 | PlayStation Game CD |
| 113 | PlayStation 2 Game CD |
| 114 | PlayStation 2 Game DVD |
| 115 | PlayStation 3 Game DVD |
| 116 | PlayStation 3 Game Blu-ray |
| 117 | PlayStation 4 Game Blu-ray |
| 118 | PlayStation Portable Universal Media Disc (ECMA-365) |
| 119 | PlayStation Vita Game Card |
| 130 | Microsoft X-box Game Disc |
| 131 | Microsoft X-box 360 Game Disc |
| 132 | Microsoft X-box 360 Game Disc 2 |
| 133 | Microsoft X-box One Game Disc |
| 150 | Sega CD / Sega MegaCD disc |
| 151 | Sega Saturn disc |
| 152 | Sega/Yamaha Gigabyte Disc (GD-ROM) |
| 153 | Sega/Yamaha Recordable Gigabyte Disc (GD-R) |
| 154 | Sega Card |
| 155 | MilCD |
| 170 | PC-Engine / TurboGrafx cartridge |
| 171 | Super CDROM2 |
| 172 | Atari Jaguar CD disc |
| 173 | 3DO disc |
| 174 | NEC PC-FX disc |
| 175 | Neo-Geo CD disc |
| 176 | Commodore CDTV disc |
| 177 | Amiga CD32 disc |
| 178 | Nuon disc |
| 179 | Bandai Playdia disc |
| 180 | Apple \]\[ 5.25” disk, 13 sectors |
| 181 | Apple \]\[ 5.25” disk, 13 sectors, two sides |
| 182 | Apple \]\[ 5.25” disk, 16 sectors |
| 183 | Apple \]\[ 5.25” disk, 16 sectors, two sides |
| 184 | Apple 3.5” disk, double density, single side |
| 185 | Apple 3.5” disk, double density, double side |
| 186 | Apple FileWare 5.25” disk |
| 190 | IBM PC 5.25” disk, 8 sectors, single side |
| 191 | IBM PC 5.25” disk, 9 sectors, single side |
| 192 | IBM PC 5.25” disk, 8 sectors, double side |
| 193 | IBM PC 5.25” disk, 9 sectors, double side |
| 194 | IBM PC 5.25” disk, high density |
| 195 | IBM PC 3.5” disk, double density, 8 sectors, single side |
| 196 | IBM PC 3.5” disk, double density, 9 sectors, single side |
| 197 | IBM PC 3.5” disk, double density, 8 sectors, double side |
| 198 | IBM PC 3.5” disk, double density, 9 sectors, double side |
| 199 | IBM PC 3.5” disk, high density |
| 200 | IBM PC 3.5” disk, extra density |
| 201 | Microsoft DMF 3.5” disk |
| 202 | Microsoft DMF 3.5” disk, 82 tracks |
| 203 | IBM XDF 5.25” disk |
| 204 | IBM XDF 3.5” disk, high density |
| 210 | IBM 23FD 8” disk |
| 211 | IBM 33FD 8” disk, 128 bytes per sector |
| 212 | IBM 33FD 8” disk, 256 bytes per sector |
| 213 | IBM 33FD 8” disk, 512 bytes per sector |
| 214 | IBM 43FD 8” disk, 128 bytes per sector |
| 215 | IBM 43FD 8” disk, 256 bytes per sector |
| 216 | IBM 53FD 8” disk, 256 bytes per sector |
| 217 | IBM 53FD 8” disk, 512 bytes per sector |
| 218 | IBM 53FD 8” disk, 1024 bytes per sector |
| 220 | DEC RX01 8” disk |
| 221 | DEC RX02 8” disk |
| 222 | DEC RX03 8” disk |
| 223 | DEC RX50 5.25” disk |
| 230 | Acorn 5.25” disk, single density, 40 tracks |
| 231 | Acorn 5.25” disk, single density, 80 tracks |
| 232 | Acorn 5.25” disk, double density, 40 tracks |
| 233 | Acorn 5.25” disk, double density, 80 tracks |
| 234 | Acorn 5.25” disk, double density, double sided, 80 tracks |
| 235 | Acorn 3.5” disk, double density |
| 236 | Acorn 3.5” disk, high density |
| 240 | Atari 5.25” disk, single density, single side |
| 241 | Atari 5.25” disk, extended density, single side |
| 242 | Atari 5.25” disk, double density, single side |
| 243 | Atari 3.5” disk, 10 sectors, single side |
| 244 | Atari 3.5” disk, 10 sectors |
| 245 | Atari 3.5” disk, 11 sectors, single side |
| 246 | Atari 3.5” disk, 11 sectors |
| 250 | Commodore 1581 3.5” floppy |
| 251 | Amiga 3.5” floppy |
| 252 | Amiga 3.5” floppy, high density |
| 253 | Commodore 1540 5.25” floppy |
| 254 | Commodore 1540 5.25” floppy, 40 tracks |
| 255 | Commodore 1571 5.25” floppy |
| 260 | NEC 8” floppy, single density |
| 261 | NEC 8” floppy, double density |
| 262 | NEC 5.25” floppy, single density, single side |
| 263 | NEC 5.25” floppy, single density, double side |
| 264 | NEC 5.25” floppy, high density |
| 265 | NEC 3.5” floppy, high density, 77 tracks |
| 266 | NEC 3.5” floppy, high density, 80 tracks |
| 267 | NEC 3.5” floppy, triple density |
| 268 | Sharp 5.25” floppy, high density |
| 269 | Sharp 3.5” floppy, high density |
| 270 | ECMA-99 5.25” floppy, 8 sectors |
| 271 | ECMA-99 5.25” floppy, 15 sectors |
| 272 | ECMA-99 5.25” floppy, 26 sectors |
| 273 | ECMA-54 8” floppy |
| 274 | ECMA-59 8” floppy |
| 275 | ECMA-66 5.25” floppy |
| 276 | ECMA-69 8” floppy, 8 sectors |
| 277 | ECMA-69 8” floppy, 15 sectors |
| 278 | ECMA-69 8” floppy, 26 sectors |
| 279 | ECMA-70 5,25” floppy |
| 280 | ECMA-78 5,25” floppy |
| 281 | ECMA-78 5,25” floppy, 9 sectors |
| 290 | FDFORMAT 5.25” floppy, double density, 82 tracks, 10 sectors |
| 291 | FDFORMAT 5.25” floppy, high density, 82 tracks, 17 sectors |
| 292 | FDFORMAT 3.5” floppy, double density, 82 tracks, 10 sectors |
| 293 | FDFORMAT 3.5” floppy, high density, 82 tracks, 21 sectors |
| 309 | Apricot 3.5” floppy |
| 310 | OnStream ADR2120 |
| 311 | OnStream ADR260 |
| 312 | OnStream ADR30 |
| 313 | OnStream ADR50 |
| 320 | AIT |
| 321 | AIT Turbo |
| 322 | AIT 2 |
| 323 | AIT 2 Turbo |
| 324 | AIT 3 |
| 325 | AIT 3 Ex |
| 326 | AIT 3 Turbo |
| 327 | AIT 4 |
| 328 | AIT 5 |
| 329 | AIT E Turbo |
| 330 | Super AIT |
| 331 | Super AIT 2 |
| 340 | Iomega Bernoulli |
| 341 | Iomega Bernoulli 2 |
| 342 | Iomega Ditto |
| 343 | Iomega Ditto MAX |
| 344 | Iomega JAZ |
| 345 | Iomega JAZ 2Gb |
| 346 | Iomega Clik! / PocketZip |
| 347 | Iomega REV 120Gb |
| 348 | Iomega REV 70Gb |
| 349 | Iomega REV 35Gb |
| 350 | Iomega ZIP |
| 351 | Iomega ZIP 250Mb |
| 352 | Iomega ZIP 750Mb |
| 360 | Philips Compact Cassette |
| 361 | Data8 |
| 362 | MiniDV |
| 363 | D/CAS-25: Digital data on Compact Cassette form factor, special magnetic media, 9-track |
| 364 | D/CAS-85: Digital data on Compact Cassette form factor, special magnetic media, 17-track |
| 365 | D/CAS-103: Digital data on Compact Cassette form factor, special magnetic media, 21-track |
| 370 | CFast |
| 371 | CompactFlash |
| 372 | CompactFlash Type 2 |
| 380 | Digital Audio Tape |
| 381 | DAT160 |
| 382 | DAT320 |
| 383 | DAT72 |
| 384 | DDS |
| 385 | DDS-2 |
| 386 | DDS-3 |
| 387 | DDS-4 |
| 390 | DEC CompacTape |
| 391 | DEC CompacTape II |
| 392 | DECtape II |
| 393 | DLTtape III |
| 394 | DLTtape IIIxt |
| 395 | DLTtape IV |
| 396 | DLTtape S4 |
| 397 | Super DLT |
| 398 | Super DLT 2 |
| 399 | VStape I |
| 400 | Exatape (15m) |
| 401 | Exatape (22m) |
| 402 | Exatape AME (22m) |
| 403 | Exatape (28m) |
| 404 | Exatape (40m) |
| 405 | Exatape (45m) |
| 406 | Exatape (54m) |
| 407 | Exatape (75m) |
| 408 | Exatape (76m) |
| 409 | Exatape (80m) |
| 410 | Exatape (106m) |
| 411 | Exatape XL (160m) |
| 412 | Exatape (112m) |
| 413 | Exatape (125m) |
| 414 | Exatape (150m) |
| 415 | Exatape (170m) |
| 416 | Exatape (225m) |
| 420 | ExpressCard (34mm) |
| 421 | ExpressCard (54mm) |
| 422 | PC-Card Type I |
| 423 | PC-Card Type II |
| 424 | PC-Card Type III |
| 425 | PC-Card Type IV |
| 430 | SyQuest EZ135 |
| 431 | SyQuest EZ230 |
| 432 | SyQuest Quest |
| 433 | SyQuest SparQ |
| 434 | SyQuest SQ100 |
| 435 | SyQuest SQ200 |
| 436 | SyQuest SQ300 |
| 437 | SyQuest SQ310 |
| 438 | SyQuest SQ327 |
| 439 | SyQuest SQ400 |
| 440 | SyQuest SQ800 |
| 441 | SyQuest SQ1500 |
| 442 | SyQuest SQ2000 |
| 443 | SyQuest SyJet |
| 450 | Nintendo Famicom Game Pak |
| 451 | Nintendo Game Boy Advance Game Pak |
| 452 | Nintendo Game Boy Game Pak |
| 453 | Nintendo Gamecube Optical Disc |
| 454 | Nintendo 64 Dynamic Disk |
| 455 | Nintendo 64 Game Pak |
| 456 | Nintendo Entertainment System Game Pak |
| 457 | Nintendo 3DS Game Card |
| 458 | Nintendo Disk Card |
| 459 | Nintendo DS Game Card |
| 460 | Nintendo DSi Game Card |
| 461 | Super Nintendo Entertainment System Game Pak |
| 462 | Super Nintendo Entertainment System Game Pak (USA) |
| 463 | Nintendo Wii Optical Disc |
| 464 | Nintendo Wii U Optical Disc |
| 465 | Nintendo Switch Game Card |
| 470 | IBM3470 |
| 471 | IBM3480 |
| 472 | IBM3490 |
| 473 | IBM3490E |
| 474 | IBM3592 |
| 480 | LTO |
| 481 | LTO-2 |
| 482 | LTO-3 |
| 483 | LTO-3 WORM |
| 484 | LTO-4 |
| 485 | LTO-4 WORM |
| 486 | LTO-5 |
| 487 | LTO-5 WORM |
| 488 | LTO-6 |
| 489 | LTO-6 WORM |
| 490 | LTO-7 |
| 491 | LTO-7 WORM |
| 510 | MemoryStick |
| 511 | MemoryStick Duo |
| 512 | MemoryStick Micro (M2) |
| 513 | MemoryStick Pro |
| 514 | MemoryStick Pro Duo |
| 520 | microSD |
| 521 | miniSD |
| 522 | Secure Digital |
| 530 | MultiMediaCard |
| 531 | MMCmicro |
| 532 | RS-MMC |
| 533 | MMCplus |
| 534 | MMCmobile |
| 540 | MLR |
| 541 | MLR SL |
| 542 | MLR-3 |
| 543 | SLR |
| 544 | SLR-2 |
| 545 | SLR-3 |
| 546 | SLR-32 |
| 547 | SLR-32 SL |
| 548 | SLR-4 |
| 549 | SLR-5 |
| 550 | SLR-5 SL |
| 551 | SLR-6 |
| 552 | SLRtape7 |
| 553 | SLRtape7 SL |
| 554 | SLRtape24 |
| 555 | SLRtape24 SL |
| 556 | SLRtape40 |
| 557 | SLRtape50 |
| 558 | SLRtape60 |
| 559 | SLRtape75 |
| 560 | SLRtape100 |
| 561 | SLRtape140 |
| 570 | QIC-11 |
| 571 | QIC-120 |
| 572 | QIC-1350 |
| 573 | QIC-150 |
| 574 | QIC-24 |
| 575 | QIC-3010 |
| 576 | QIC-3020 |
| 577 | QIC-3080 |
| 578 | QIC-3095 |
| 579 | QIC-320 |
| 580 | QIC-40 |
| 581 | QIC-525 |
| 582 | QIC-80 |
| 590 | STK4480 |
| 591 | STK4490 |
| 592 | STK9490 |
| 593 | T9840A |
| 594 | T9840B |
| 595 | T9840C |
| 596 | T9840D |
| 597 | T9940A |
| 598 | T9940B |
| 599 | T10000A |
| 600 | T10000B |
| 601 | T10000C |
| 602 | T10000D |
| 610 | Travan |
| 611 | Travan Ex |
| 612 | Travan 3 |
| 613 | Travan 3 Ex |
| 614 | Travan 4 |
| 615 | Travan 5 |
| 616 | Travan 7 |
| 620 | VXA |
| 621 | VXA-2 |
| 622 | VXA-3 |
| 630 | 5.25” magneto-optical, ECMA-153, ISO 11560, 1024 bytes per sector |
| 631 | 5.25” magneto-optical, ECMA-153, ISO 11560, 512 bytes per sector |
| 632 | 3.5” magneto-optical, ECMA-154, ISO 10090, 512 bytes per sector |
| 633 | 5.25” magneto-optical, ECMA-183, ISO 13481, 512 bytes per sector |
| 634 | 5.25” magneto-optical, ECMA-183, ISO 13481, 1024 bytes per sector |
| 635 | 5.25” magneto-optical, ECMA-184, ISO 13549, 512 bytes per sector |
| 636 | 5.25” magneto-optical, ECMA-184, ISO 13549, 1024 bytes per sector |
| 637 | 300mm magneto-optical, ECMA-189, ISO 13614 |
| 638 | 300mm magneto-optical, ECMA-190, ISO 13403 |
| 639 | 5.25” magneto-optical, ECMA-195, ISO 13842, 1024 bytes per sector |
| 640 | 5.25” magneto-optical, ECMA-195, ISO 13842, 512 bytes per sector |
| 641 | 3.5” magneto-optical, ECMA-201, ISO 13963 |
| 642 | 3.5” magneto-optical, ECMA-201, ISO 13963, embossed |
| 643 | 3.5” magneto-optical, ECMA-223, 1024 bytes per sector |
| 644 | 3.5” magneto-optical, ECMA-223, 512 bytes per sector |
| 645 | 5.25” magneto-optical, ECMA-238, ISO 15486 |
| 646 | 3.5” magneto-optical, ECMA-239, ISO 15498 |
| 647 | 356mm magneto-optical, ECMA-260, ISO 15898 |
| 648 | 356mm magneto-optical, ECMA-260, ISO 15898, double size |
| 649 | 5.25” magneto-optical, ECMA-280, ISO 18093 |
| 650 | 300mm magneto-optical, ECMA-317, ISO 20162 |
| 651 | 5.25” magneto-optical, ECMA-322, ISO 22092, 4096 bytes per sector |
| 652 | 5.25” magneto-optical, ECMA-322, ISO 22092, 2048 bytes per sector |
| 653 | 3.5” magneto-optical, Cherry Book, GigaMO, ECMA-351, ISO 17346 |
| 654 | 3.5” magneto-optical, Cherry Book 2, GigaMO 2, ECMA-353, ISO 22533 |
| 660 | 3” CompactFloppy |
| 661 | IBM 4” DemiDiskette floppy |
| 662 | Insite 3.5” Floptical, ECMA-207, ISO 14169 |
| 663 | Sony 3.5” HiFD floppy |
| 664 | Mitsumi 3” Quick Disk |
| 665 | Caleb 3.5” UHD144 floppy |
| 666 | Canon VideoFloppy |
| 667 | Wafer |
| 668 | ZX Microdrive |
| 670 | BeeCard |
| 671 | Borsu |
| 672 | DataStore |
| 673 | DIR |
| 674 | DST |
| 675 | DTF |
| 676 | DTF 2 |
| 677 | Flextra 3020 |
| 678 | Flextra 3225 |
| 679 | HiTC |
| 680 | HiTC 2 |
| 681 | LT-1 |
| 682 | MiniCard |
| 683 | Orb |
| 684 | Orb 2 |
| 685 | SmartMedia |
| 686 | xD memory card |
| 687 | XQD |
| 688 | DataPlay |
| 690 | Apple Profile |
| 691 | Apple Widget |
| 692 | Apple HD20 (not SCSI) |
| 693 | Priam Data Tower |
| 694 | Apple Pippin disc |
| 700 | DEC RA60 |
| 701 | DEC RA80 |
| 702 | DEC RA81 |
| 703 | DEC RC25 |
| 704 | DEC RD31 |
| 705 | DEC RD32 |
| 706 | DEC RD51 |
| 707 | DEC RD52 |
| 708 | DEC RD53 |
| 709 | DEC RD54 |
| 710 | DEC RK06 |
| 711 | DEC RK06 (18 bits per word) |
| 712 | DEC RK07 |
| 713 | DEC RK07 (18 bits per word) |
| 714 | DEC RM02 |
| 715 | DEC RM03 |
| 716 | DEC RM05 |
| 717 | DEC RP02 |
| 718 | DEC RP02 (18 bits per word) |
| 719 | DEC RP03 |
| 720 | DEC RP03 (18 bits per word) |
| 721 | DEC RP04 |
| 722 | DEC RP04 (18 bits per word) |
| 723 | DEC RP05 |
| 724 | DEC RP05 (18 bits per word) |
| 725 | DEC RP06 |
| 726 | DEC RP06 (18 bits per word) |
| 730 | Imation LS-120 3.5” floppy |
| 731 | Imation LS-240 3.5” floppy |
| 732 | Sony 3.5” floppy formatted in Imation LS-240 drive |
| 733 | RDX |
| 734 | RDX 320Gb |
| 740 | Hasbro/Tiger VideoNow |
| 741 | Hasbro/Tiger VideoNow Color |
| 742 | Hasbro/Tiger VideoNow XP |


## **Dump status flags**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0x00 | The dump status is unknown. |
| 0x01 | The dump has been stripped of the copy protection (cracked). |
| 0x02 | ***The dump has been manually fixed.** |
| 0x04 | The dump has been intentionally modified (hacked). |
| 0x08 | The dump has been unintentionally modified (e.g. marked by Windows 95). |
| 0x10 | The dump is piracy, in a way non described by other flags. |
| 0x20 | The dump has been modified to add a trainer or cheat systems, or to modify the difficulty altogether. |
| 0x40 | The dump has been unofficially translated to another language. |
| 0x80 | The dump contains more data than the media indicates it should. |
| 0x100 | The dump contains less data than the media indicates it should. |
| 0x200 | The dump contains known malware (it is infected). |
| 0x400 | The media used to create the dump contains read errors of any kind (does not include Compact Disc subchannel errors). |
| 0x800 | Different dumpers have achieved the exact same dump using different media (verified). |
| 0x1000 | The dump is missing side data and it is not known if this data is essential for the media (e.g. a Compact Disc dump with no subchannel, Apple double density floppies with no sector tags). |
| 0x2000 | The dump is missing side data and it is known this data is not essential for the media (e.g. a Compact Disc dump with no subchannel that is known to only have standard, replicable, values in it.) |
| 0x4000 | The dump is missing side data and it is known this data is essential for the media (e.g. a libcrypt protected PlayStation dump with no subchannel data dumped, an Apple Lisa disk missing the sector tags). |
| 0x8000 | The dump belongs to a CompactDisc and the subchannel contains non-intentional (that is, not part of a copy protection, etc), non-recovered damage. |


## **Subchannel flags**

**	This enumeration lists the subchannels dump status.**


| **Value** | **Description** |
| :-: | :-: |
| 0x00 | No subchannel has been dumped |
| 0x01 | P subchannel is present in the dump. |
| 0x02 | ***Q subchannel is present in the dump.** |
| 0x04 | ***R subchannel is present in the dump.** |
| 0x08 | ***S subchannel is present in the dump.** |
| 0x10 | ***T subchannel is present in the dump.** |
| 0x20 | ***U subchannel is present in the dump.** |
| 0x40 | ***V subchannel is present in the dump.** |
| 0x80 | ***W subchannel is present in the dump.** |


## **File attributes**

**	This enumeration lists the possible attributes a file can have. It is maintained in sync with Aaru file attributes enumeration.**


| **Value** | **Description** |
| :-: | :-: |
| 0x0000000000000 | File has no attributes. |
| 0x0000000000001 | File is an alias (Mac OS). |
| 0x0000000000002 | ***Indicates that the file can only be writable appended**. |
| 0x0000000000004 | File is candidate for archival/backup. |
| 0x0000000000008 | File is a block device. |
| 0x0000000000010 | File is stored on filesystem block units instead of device sectors. |
| 0x0000000000020 | Directory is a bundle for file contains a *BNDL* resource. |
| 0x0000000000040 | File is a character device. |
| 0x0000000000080 | File is compressed. |
| 0x0000000000100 | File is compressed and should not be uncompressed on read. |
| 0x0000000000200 | File has compression errors. |
| 0x0000000000400 | Compressed file is dirty. |
| 0x0000000000800 | File is a device. |
| 0x0000000001000 | File is a directory. |
| 0x0000000002000 | File is encrypted. |
| 0x0000000004000 | File is stored on disk using extents. |
| 0x0000000008000 | File is a FIFO. |
| 0x0000000010000 | File is a normal file. |
| 0x0000000020000 | File is a Mac OS file containing desktop databases that have already been added to the desktop database. |
| 0x0000000040000 | File contains an icon resource or extended attribute. |
| 0x0000000080000 | File is a Mac OS extension or control panel lacking INIT resources. |
| 0x0000000100000 | File is hidden/invisible. |
| 0x0000000200000 | File cannot be written, deleted, modified or linked to. |
| 0x0000000400000 | Directory is indexed using hashed trees. |
| 0x0000000800000 | File contents are stored alongside its inode (or equivalent). |
| 0x0000001000000 | File contains integrity checks. |
| 0x0000002000000 | File is on desktop. |
| 0x0000004000000 | File changes are written to filesystem journal before being written to the file itself. |
| 0x0000008000000 | Access time will not be modified. |
| 0x0000010000000 | File will not be subjet to copy-on-write. |
| 0x0000020000000 | File will not be backed up. |
| 0x0000040000000 | File contents should not be scrubbed. |
| 0x0000080000000 | File contents should not be indexed. |
| 0x0000100000000 | File is offline. |
| 0x0000200000000 | File is password protected, but contents are not encrypted on disk. |
| 0x0000400000000 | File is read-only. |
| 0x0000800000000 | File is a reparse point. |
| 0x0001000000000 | When file is removed its content will be overwritten with zeroes. |
| 0x0002000000000 | File contents are sparse. |
| 0x0004000000000 | File is a shadow (OS/2). |
| 0x0008000000000 | File is shared. |
| 0x0010000000000 | File is a stationery. |
| 0x0020000000000 | File is a symbolic link. |
| 0x0040000000000 | File writes are synchronously written to disk. |
| 0x0080000000000 | File belongs to the operating system. |
| 0x0100000000000 | If file end is a partial block its content will be merged with other files. |
| 0x0200000000000 | File is temporary. |
| 0x0400000000000 | Subdirectories inside of this directory are not related and should be allocated elsewhere |
| 0x0800000000000 | If file is deleted, contents should be stored, for a possible future undeletion. |
| 0x1000000000000 | File is a pipe. |
| 0x2000000000000 | File is a socket. |


## **Media tag type**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


## **Distribution mode**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


## **SoundSynthType**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


## **TrackType**

**	This enumeration lists the photo sharpness defined by EXIF.**


| **Value** | **Description** |
| :-: | :-: |
| 0 | Normal. |
| 1 | Low. |
| 2 | ***High**. |


# **Examples**

The purpose of this section is to give real life examples of the artifacts described in the previous sections.
