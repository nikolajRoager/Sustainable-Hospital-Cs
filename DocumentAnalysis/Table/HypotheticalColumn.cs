using StringAnalyzer;

namespace DocumentAnalysis
{

    /// <summary>
    /// A hypothetical column in the analyzed, which may still be somewhat ambiguos as to what it is
    /// </summary>
    /*package private*/class HypotheticalColumn 

    {
        /// <summary>
        /// Clone the content of this column into that column
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public HypotheticalColumn CloneContentTo(HypotheticalColumn that)
        {
            that.header_y=header_y;       
            that.column_x=column_x;       
            that.height  =height;
            return that;
        }

        /// <summary>
        /// How many different things can this column be
        /// </summary>
        public int ambiguoity { get{return (couldBeNumber?1:0)+(couldBeProduct?1:0)+(couldBeSingleMass?1:0)+(couldBeAmount?1:0)+(couldBeTotalMass?1:0);} }
        public bool couldBeProduct { get; set; } = false;
        public bool couldBeNumber { get; set; } = false;
        public bool couldBeSingleMass { get; set; } = false;
        public bool couldBeTotalMass { get; set; } = false;
        public bool couldBeAmount {get; set;} = false;

        /// <summary>
        /// A binary representation of the state of this column, which can both be set and written to
        /// </summary>
        public int signature
        {
            get
            {
                return
                ( couldBeProduct    ? 0b10000 : 0 ) |
                ( couldBeNumber     ? 0b01000 : 0 ) |
                ( couldBeAmount     ? 0b00100 : 0 ) |
                ( couldBeSingleMass ? 0b00010 : 0 ) |
                ( couldBeTotalMass  ? 0b00001 : 0 ) ;
            }
            set
            {
                couldBeProduct    = (0b10000 & value) != 0;
                couldBeNumber     = (0b01000 & value) != 0;
                couldBeAmount     = (0b00100 & value) != 0;
                couldBeSingleMass = (0b00010 & value) != 0;
                couldBeTotalMass  = (0b00001 & value) != 0;
            }
        }

        public int header_y { get; set; } 
        public int column_x { get; set;} 
        public int height { get; set; } = 0;

    }

}