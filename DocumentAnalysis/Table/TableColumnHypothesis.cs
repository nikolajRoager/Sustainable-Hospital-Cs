namespace DocumentAnalysis
{

    /// <summary>
    /// A hypothetical column in the analyzed, which may still be somewhat ambiguos as to what it is
    /// </summary>
    /*package private*/class TableColumnHypothesis

    {
        public bool couldBeProduct { get; set; }
        public bool couldBeNumber { get; set; }
        public bool couldBeSingleMass { get; set; }
        public bool couldBeTotalMass { get; set; }
        public bool couldBeAmount {get; set;}
        /// <summary>
        /// A column may be spread out over multiple columns for some stupid reason
        /// </summary>
        public bool isSpreadOut { get; set; }
        public int header_y { get; set; }
        public int column_x { get; set;}
        public int height { get; set; }
    }

}