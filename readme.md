New Sustainable Hospital, C#
=========
Portion of this software uses third party packages and resources, see credit and licenses at the end of this document.

A new implementation of Sustainable hospital, this program merges Excel documents, storing products used by different cantinas at various hospitals, with the purpose of tracking Carbon emissions.


The program has a number of modes:

In **merge** mode the program attempts to correctly identify what category each product in any number of supplied Excel belongs to, and create an Excel document with all products of these and all previous documents belong to; the program may ask the user for clarification in some cases.

In **retrain** mode the program re-learns how to identify the categories, based on an Excel file with categories, ingredients, and keywords, using a provided synonym dictionary.

Note, the product is developed for a Danish customer, and thus the products and categories in the example Excel documents, and all printed text from the program are in Danish.

Retraining with alternative synonym dictionary
---------------
The Synonym dictionary included with this program is `ddo-synonyms.csv` provided by DSL (see license and link below), alternative synonym dictionary files can be supplied by running with the flag `-sd filename` must follow the same standard as this file:

The file is a CSV file, where a word or phrase is separated from its synonyms by a tab `'\t'` (I would have saved that as .tsv, but this is how it was supplied by DSL). Synonyms are separated by `';'` Paranthesis indicate optional words. The file MUST be in alphabetic order! 

Detecting and merging Excel columns
-------------------
The greatest challenge is analyzing excel documents with different format, and extracting the required data.

The different customers obviously use different names for the same things, and they may split up the tables over multiple different tables, like this:


| cantina 1  |        |            |
| ---------- | ------ | ---------- |
| product    | amount | total kg   |
| CVN        |  1     |   2.4      |
| F35        |  2     |   4.5      |
| Car        |  3     |   0.2      |
| Cat        |  5     |   2.0      |
|            |        |            |
| cantina 2  |        |            |
| product    | amount | total kg   |
| Trireme    |  1     |  10.0      |
| Spain      |  2     |   4.0      |
| Battleship |  3     |   0.3      |
| Cat        |  2     |   0.8      |

Some people might even include some information in the same column, i.e. someone could write ("4 Preteens (60kg)") in the product name column, and we will have to figure out if those 60 kg are the mass of one preteen, or the mass of all 4 of them.

To analyze the data, the program simply looks at every row, and tries to classify them based on their content, using the RegexStringAnalyzer class, which identifies the names of products, integers, floating numbers, and masses directly in a string

CREDITS and LICENSES
=====

Det Danske Sprog og Litteraturselskab DSL.
----
This program relies on a list of synonyms  `ddo_synonyms.csv` based on "Den Danske Ordbog"/"The Danish Dictionary" developed by "DSL: Det Danske Sprog og Litteraturselskab"/"DSL: the Society for Danish Language and Literature".

This product is not in any way endorsed by DSL!

Also, please **DO NOT!** use this Github repository as a source for Danish Synonyms! The file `ddo_synonyms.csv` was downloaded from [https://korpus.dsl.dk/resources/index.html](https://korpus.dsl.dk/resources/index.html) on April 25, 2025, and may be out of date.


Original license from DSL: [https://korpus.dsl.dk/resources/licences/dsl-open.html#list](https://korpus.dsl.dk/resources/licences/dsl-open.html#list)

>     You are free to use The Society of Danish Language and Literature's open language resources for most purposes, even commercially. You are free to edit and build upon the resources as well as redistribute them in any medium or format.
>        However, you may not use the resources to publish a dictionary in any form or any other product which is in competition with DSL's own products.
>
>    You are requested to give appropriate credit to the The Society for Danish Language and Literature, DSL, in publications and digital products based entirely or partly on these language resources and provide a link to dsl.dk. You may do so in any reasonable manner, but not in any way that suggests The Society for Danish Language and Literature, DSL, endorses you or your use of the resources. The Society for Danish Language and Literature, DSL, appreciates to receive either a copy of a resulting product or publication or a link to it.


EPPlus package
-----------
NOTE, For now, during debugging and testing, I use a personal non-commercial license for the EPPlus package. For loading Excel packages.


Notice while under development
====
When under development, the program uses commandline interface; this will be changed before final
