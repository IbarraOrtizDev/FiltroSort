namespace FilterSortUnitTest
{
    public class TestFilterSortQueryAllSearchable
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
            filter.Filter = "Texto18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = ValidateFilterDataContains("Texto18");
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsDate()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "1/01/2022";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = ValidateFilterDataContains("1/01/2022");

            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsNumber()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "18";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = ValidateFilterDataContains("18");
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }
        [Test]
        public void ValidateFilterDataContainsBoolean()
        {
            //arrange
            FilterSoftModel filter = new FilterSoftModel();
            filter.Filter = "true";
            //act
            FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
            var filt = filterSort.GetFilterExpression();
            var data = _data.AsQueryable().Where(filt).ToList();
            var dataCount = ValidateFilterDataContains("true");
            //assert
            Assert.AreEqual(dataCount, data.Count);
        }

        private int ValidateFilterDataContains(string validar)
        {
            var dataCount = _data.Where(x => x.propiedadString.ToString().ToLower().Contains(validar.ToLower())
                       || x.propiedadEntera.ToString().ToLower().Contains(validar.ToLower()) 
                                  || x.propiedadBooleana.ToString().ToLower().Contains(validar.ToLower())
                                             || x.propiedadFecha.ToString().ToLower().Contains(validar.ToLower())).ToList();

            return dataCount.Count;
        }
    }
}
