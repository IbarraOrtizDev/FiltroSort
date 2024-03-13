namespace FilterSortUnitTest
{
    public class TestFilterSortQueryEquals
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
        public void ValidateFilterDataEqualsString()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==Test18";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString == "Test18").ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataEqualsStringTwo()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadString==test18";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadString == "test18").ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }

        [Test]
        public void ValidateFilterDataEqualsNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadEntera==12";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadEntera == 12).ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
            Assert.That(dataCount[0].propiedadString, Is.EqualTo(data[0].propiedadString));
        }

        [Test]
        public void ValidateFilterDataEqualsBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadBooleana==true";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadBooleana == true).ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
        [Test]
        public void ValidateFilterDataEqualsDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "propiedadFecha==2022-01-01";

            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = _data.Where(x => x.propiedadFecha == Convert.ToDateTime("2022-01-01")).ToList();

            //assert
            Assert.AreEqual(dataCount.Count, data.Count);
        }
    }
}