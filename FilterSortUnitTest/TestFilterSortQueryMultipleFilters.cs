namespace FilterSortUnitTest
{
    public class TestFilterSortQueryMultipleFilters
    {
        List<DataDTO> _data;
        [SetUp]
        public void Setup()
        {
            ServiceData serviceData = new ServiceData();
            _data = serviceData.GetData();
        }

        [Test]
        public void ValidateContainData()
        {
            Assert.AreNotEqual(0, _data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleFiltersBoolAndString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==Texto18,propiedadBooleana==true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString == "Texto18" && x.propiedadBooleana==true).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleFiltersBoolAndStringTwo()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString_=Texto1,propiedadBooleana==true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.StartsWith("Texto1") && x.propiedadBooleana == true).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleFiltersBoolStringAndDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString_=Texto1,propiedadBooleana==true,propiedadFecha==2022-01-01";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.StartsWith("Texto1") && x.propiedadBooleana == true && x.propiedadFecha == Convert.ToDateTime("2022-01-01")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleFiltersBoolStringAndDateContains()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString@=xto18,propiedadBooleana==true,propiedadFecha==2022-01-01";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.Contains("xto18") && x.propiedadBooleana == true && x.propiedadFecha == Convert.ToDateTime("2022-01-01")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataMultipleFiltersBoolStringAndDateContainsNotBool()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString@=xto18,propiedadBooleana=true,propiedadFecha==2022-01-01";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.Contains("xto18") && x.propiedadFecha == Convert.ToDateTime("2022-01-01")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleFiltersGreaterAndBool()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera>=1000,propiedadFecha==2022-01-01";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadEntera >= 1000 && x.propiedadFecha == Convert.ToDateTime("2022-01-01")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataMultipleRangeDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha>=2022-01-01,propiedadFecha<=2022-01-15";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadFecha >= Convert.ToDateTime("2022-01-01") && x.propiedadFecha <= Convert.ToDateTime("2022-01-15")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
    }
}
