namespace FilterSortUnitTest
{
    public class TestFilterSortQueryArrayContains
    {
        //propiedadString==Texto18|Texto19
        //propiedadEntera==18|19|20|50
        //propiedasdFecha==2020-01-01|2020-01-02
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
            filter.Filter = "propiedadString==Texto18|Texto19";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var filters = new List<string> { "Texto18", "Texto19" };
            var dataCount = _data.Where(x => filters.Contains(x.propiedadString)).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha==2022-01-01|2022-01-02,propiedadBooleana==true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();

            var filters = new List<DateTime> { DateTime.Parse("2022-01-01"), DateTime.Parse("2022-01-02") };
            var dataCount = _data.Where(x => filters.Contains(x.propiedadFecha) && x.propiedadBooleana==true).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera==18|19|20|50";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var filters = new List<int> { 18, 19, 20, 50 };
            var dataCount = _data.Where(x => filters.Contains(x.propiedadEntera)).ToList();
            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
    }
}
