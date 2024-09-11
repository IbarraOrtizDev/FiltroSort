namespace FilterSortUnitTest
{
    class TestFilterSortQueryAndOr
    {
        List<DataDTO> _data;
        [SetUp]
        public void Setup()
        {
            ServiceData serviceData = new ServiceData();
            _data = serviceData.GetData();
        }

        [Test]
        public void ValidateOr()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "|propiedadString==Texto18,propiedadEntera==4998";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            var dataCount = _data.Where(x => x.propiedadString == "Texto18" || x.propiedadEntera == 4998).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateAnd()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==Texto4998,propiedadEntera==4998";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            var dataCount = _data.Where(x => x.propiedadString == "Texto4998" && x.propiedadEntera == 4998).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateAndOr()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==Texto4998|Texto18|Texto21|Texto32,(|propiedadEntera==4998,propiedadEntera==18,propiedadEntera==32),propiedadBooleana==true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            var dataCount = _data.Where(x => (x.propiedadString == "Texto4998" || x.propiedadString == "Texto18" || x.propiedadString == "Texto21" || x.propiedadString == "Texto32") && (x.propiedadEntera == 4998 || x.propiedadEntera == 4998 || x.propiedadEntera == 18 || x.propiedadEntera == 21 || x.propiedadEntera == 32) && x.propiedadBooleana == true).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
    }
}
