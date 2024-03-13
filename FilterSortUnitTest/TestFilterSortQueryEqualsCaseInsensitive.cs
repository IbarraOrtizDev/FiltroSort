namespace FilterSortUnitTest
{
    public class TestFilterSortQueryEqualsCaseInsensitive
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
        public void ValidateFilterDataEqualsCaseInsensitiveString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==*TEXTO18";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString.ToLower() == "TEXTO18".ToLower()).ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataEqualsCaseInsensitiveDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha==*2022-01-01";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataEqualsCaseInsensitiveNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera==*12";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataEqualsCaseInsensitiveBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBooleana==*true";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            //assert
            Assert.AreEqual(_data.Count, data.Count);
        }
    }
}
