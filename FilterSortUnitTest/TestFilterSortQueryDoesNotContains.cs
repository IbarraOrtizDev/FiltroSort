namespace FilterSortUnitTest;

public class TestFilterSortQueryDoesNotContains
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
    public void ValidateFilterDataDoesNotContainsString()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadString!@=Texto1";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();
        var dataCount = _data.Where(x => x.propiedadString.Contains("Texto1")).ToList();

        var cantidadEsperada = _data.Count-dataCount.Count;

        //assert
        Assert.AreEqual(cantidadEsperada, data.Count);

    }
    [Test]
    public void ValidateFilterDataDoesNotContainsDate()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadFecha!@=2022-01-01";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();

        //assert
        Assert.AreEqual(_data.Count, data.Count);
    }
    [Test]
    public void ValidateFilterDataDoesNotContainsNumber()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadEntera!@=18";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();

        //assert
        Assert.AreEqual(_data.Count, data.Count);
    }
    [Test]
    public void ValidateFilterDataDoesNotContainsBoolean()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "propiedadBooleana!@=true";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();

        //assert
        Assert.AreEqual(_data.Count, data.Count);
    }
}
