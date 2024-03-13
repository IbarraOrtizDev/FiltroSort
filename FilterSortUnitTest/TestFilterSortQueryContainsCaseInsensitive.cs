namespace FilterSortUnitTest
{
    public class TestFilterSortQueryContainsCaseInsensitive
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
        public void ValidateFilterDataContainsCaseInsensitiveString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString@=*texto18";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.ToLower().Contains("texto18".ToLower())).ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsCaseInsensitiveDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha@=*2022-01-01";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsCaseInsensitiveNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera@=*12";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsCaseInsensitiveBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBoolean@=*true";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
    }
}
