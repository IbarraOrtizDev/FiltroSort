namespace FilterSortUnitTest;

public class TestFilterSortQueryEnum
{
    List<DataDTO> _data;
    [SetUp]
    public void Setup()
    {
        ServiceData serviceData = new ServiceData();
        _data = serviceData.GetData();
    }

    [Test]
    public void ValidateFilterDataType()
    {
        //arrange
        FilterSoftModel filter = new FilterSoftModel();
        filter.Filter = "type==1";

        //act
        FilterSort<DataDTO> filterSort = new FilterSort<DataDTO>(filter);
        var filt = filterSort.GetFilterExpression();
        var data = _data.AsQueryable().Where(filt).ToList();
        var dataCount = _data.Where(x => x.type == EnumTestType.Boolean).ToList();

        //assert
        Assert.AreEqual(dataCount.Count, data.Count);
    }
}
