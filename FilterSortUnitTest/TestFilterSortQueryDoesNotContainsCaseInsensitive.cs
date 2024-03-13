namespace FilterSortUnitTest
{
    public class TestFilterSortQueryDoesNotContainsCaseInsensitive
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
        public void ValidateFilterDataDoesNotContainsCaseInsensitiveString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString!@=*TEXTO18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => !x.propiedadString.ToLower().Contains("TEXTO18".ToLower())).ToList();

            var dataCountTwo = _data.Where(x => x.propiedadString.ToLower().Contains("TEXTO18".ToLower())).ToList();
            var newVal = _data.Count - dataCountTwo.Count;
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
            Assert.AreEqual(newVal, data.Count);
        }
        [Test]
        public void ValidateFilterDataDoesNotContainsCaseInsensitiveDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha!@=*2022-01-01";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataDoesNotContainsCaseInsensitiveNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera!@=*12";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataDoesNotContainsCaseInsensitiveBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBooleano!@=*true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
    }
}
