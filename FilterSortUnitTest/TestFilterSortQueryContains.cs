namespace FilterSortUnitTest
{
    public class TestFilterSortQueryContains
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
        public void ValidateFilterDataContainsString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString@=Texto18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.Contains("Texto18")).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha@=01-01-2022";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            var resp = _data.Where(x => x.propiedadFecha.Value.ToString("MM-dd-yyyy").Contains("01-01-2022")).ToList();
            //assert
            Assert.AreEqual(resp.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera@=18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBooleana@=true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
    }
}
