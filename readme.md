New Sustainable Hospital, C#
=========
Portion of this software uses third party packages and resources, see credit and licenses at the end of this document.

A new implementation of Sustainable hospital, this program merges Excel documents, storing products used by different cantinas at various hospitals, with the purpose of tracking Carbon emissions.

This program is not finished, there are several problems which need to be solved, outlined at the end of the program.

The program has a number of modes:

In **normal** mode the program attempts to correctly identify what category each product in any number of supplied Excel belongs to, and create an Excel document with all products of these and all previous documents belong to; the program may ask the user for clarification in some cases.

In **retrain** mode the program re-learns how to identify the categories, based on an Excel file with categories, ingredients, and keywords, using a provided synonym dictionary. To run in retrain mode, run with the falg `--train` to train.

Note, the product is developed for a Danish customer, and thus the products and categories in the example Excel documents, and all printed text from the program are in Danish.

Retraining with alternative synonym dictionary
---------------
The Synonym dictionary included with this program is `ddo-synonyms.csv` provided by DSL (see license and link below), alternative synonym dictionary files can be supplied by running with the flag `-sd filename` must follow the same standard as this file:

The file is a CSV file, where a word or phrase is separated from its synonyms by a tab `'\t'` (I would have saved that as .tsv, but this is how it was supplied by DSL). Synonyms are separated by `';'` Paranthesis indicate optional words. The file MUST be in alphabetic order! 

Normal mode: Detecting and merging Excel columns in 3 passes
-------------------
To run the program on all the excel files in a folder, or on a single file run with the argument `--file-or-folder FILEPATH`, where `FILEPATH` is the file or folder you want to work on, the program will create a copy of the folder structure in the `out/` directory (which it also will create if it doesn't exist already). The output Excel files will be a copy of the original file, with all analysis included.

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

Some idiots might even include some information in the same column, i.e. someone could write ("4 Preteens (60kg)") in the product name column

Solving this is done in 3 passes:

* in the first pass, we look at the cells individually, and try to get an idea what each cell is

* in the second pass, we try to get an idea where the tables are, and which column are which and where

* in the third pass we look at the columns in totallity, verifying that all columns are found, and that there is logical consistency (for example, total weight must be amount times unit weight if we found all)

In all passes, the program may ask the user for help in the commandline, for instance asking if a cell does or does not contain a product name, or asking the user to pick one of a couple possible tables.

The output from the first pass is stored as a sheet in the output document, but colour coded based on what the program thinks each cell is, for instance it might believe there is a 10% chance that something is random filler, 40% chance it is a product name, and 50% chance it is the header for all products.

The output from the second pass is a sheet with the table and columns highlighted and clearly named.

Finally the output from the final pass is a sheet with a completely new table in a standardized formaat.


Problems, and future development
------------------

The example file `AC/AC.xlsx` is not working, the product column is not correctly identified in pass 2.

A new User interface is required, the console is good enough for debugging and developing, but the structure of the program allows a more modern frontend to be injected, this should be done.

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
