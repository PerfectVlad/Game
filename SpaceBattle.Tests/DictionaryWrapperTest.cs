using System.Collections;

namespace SpaceBattle.Tests
{
    public class DictionaryWrapperTests
    {
        private readonly Dictionary<string, object> _baseDict = new()
        {
            ["existing"] = "base_value",
            ["common"] = "base_common"
        };

        private readonly Dictionary<string, Func<object>> _behavior = new()
        {
            ["dynamic"] = () => "behavior_value",
            ["common"] = () => "behavior_common"
        };

        [Fact]
        public void Getter_ReturnsBehaviorValue_WhenKeyExistsInBehavior()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Equal("behavior_value", wrapper["dynamic"]);
        }

        [Fact]
        public void Getter_ReturnsBaseValue_WhenKeyNotInBehavior()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Equal("base_value", wrapper["existing"]);
        }

        [Fact]
        public void Getter_BehaviorHasPriority_WhenKeyInBoth()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Equal("behavior_common", wrapper["common"]);
        }

        [Fact]
        public void Getter_ThrowsKeyNotFoundException_WhenKeyMissing()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Throws<KeyNotFoundException>(() => wrapper["missing"]);
        }

        [Fact]
        public void Setter_UpdatesBaseDict_ForNormalKey()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            wrapper["existing"] = "new_value";
            Assert.Equal("new_value", _baseDict["existing"]);
        }

        [Fact]
        public void Setter_Throws_WhenKeyReservedForBehavior()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Throws<InvalidOperationException>(() => wrapper["dynamic"] = "new_value");
        }

        [Fact]
        public void Add_Throws_WhenKeyReservedForBehavior()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Throws<ArgumentException>(() => wrapper.Add("dynamic", "value"));
        }

        [Fact]
        public void Remove_Throws_WhenKeyReservedForBehavior()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Throws<InvalidOperationException>(() => wrapper.Remove("dynamic"));
        }

        [Fact]
        public void ContainsKey_ReturnsTrue_ForBehaviorKey()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.True(wrapper.ContainsKey("dynamic"));
            Assert.True(wrapper.ContainsKey("existing"));
        }

        [Fact]
        public void TryGetValue_ReturnsTrue_ForBehaviorKey()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.True(wrapper.TryGetValue("dynamic", out var value));
            Assert.Equal("behavior_value", value);
        }

        [Fact]
        public void Values_ContainsAllValues()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            var values = wrapper.Values;
            Assert.Contains("base_value", values);
            Assert.Contains("behavior_value", values);
            Assert.Contains("behavior_common", values); // Приоритет behavior
        }

        [Fact]
        public void Count_ReturnsCombinedUniqueKeyCount()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            Assert.Equal(3, wrapper.Count); // existing, common, dynamic
        }

        [Fact]
        public void Clear_OnlyAffectsBaseDictionary()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            wrapper.Clear();
            Assert.Empty(_baseDict);
            Assert.True(wrapper.ContainsKey("dynamic")); // Behavior keys remain
        }

        [Fact]
        public void Enumerator_YieldsAllItems()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            var result = new Dictionary<string, object>();
            foreach (var kvp in wrapper)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            Assert.Equal("behavior_value", result["dynamic"]);
            Assert.Equal("base_value", result["existing"]);
            Assert.Equal("behavior_common", result["common"]);
        }

        [Fact]
        public void CopyTo_CopiesAllItems()
        {
            var wrapper = new DictionaryWrapper(_baseDict, _behavior);
            var array = new KeyValuePair<string, object>[3];
            wrapper.CopyTo(array, 0);

            var result = new Dictionary<string, object>();
            foreach (var kvp in array)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void IsReadOnly_ShouldReflectBaseDictionary()
        {
            // Arrange
            var readOnlyDict = new Dictionary<string, object> { ["test"] = "value" }.AsReadOnly();
            var wrapper = new DictionaryWrapper(readOnlyDict, new Dictionary<string, Func<object>>());

            // Act & Assert
            Assert.True(wrapper.IsReadOnly);
        }

        [Fact]
        public void IsReadOnly_ShouldBeFalseForWritableDictionary()
        {
            // Arrange
            var wrapper = new DictionaryWrapper(new Dictionary<string, object>(), new Dictionary<string, Func<object>>());

            // Act & Assert
            Assert.False(wrapper.IsReadOnly);
        }

        [Fact]
        public void AddKeyValuePair_ShouldAddToBaseDictionary()
        {
            // Arrange
            var dict = new Dictionary<string, object>();
            var wrapper = new DictionaryWrapper(dict, new Dictionary<string, Func<object>>());
            var item = new KeyValuePair<string, object>("newKey", "newValue");

            // Act
            wrapper.Add(item);

            // Assert
            Assert.Equal("newValue", dict["newKey"]);
        }

        [Fact]
        public void AddKeyValuePair_ShouldThrowForBehaviorKey()
        {
            // Arrange
            var behavior = new Dictionary<string, Func<object>> { ["reserved"] = () => "null" };
            var wrapper = new DictionaryWrapper(new Dictionary<string, object>(), behavior);
            var item = new KeyValuePair<string, object>("reserved", "value");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => wrapper.Add(item));
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldReturnTrueForExistingItem()
        {
            // Arrange
            var dict = new Dictionary<string, object> { ["key"] = "value" };
            var wrapper = new DictionaryWrapper(dict, new Dictionary<string, Func<object>>());
            var item = new KeyValuePair<string, object>("key", "value");

            // Act & Assert
            var res = wrapper.Contains(item);

            Assert.True(res);
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldReturnFalseForWrongValue()
        {
            // Arrange
            var dict = new Dictionary<string, object> { ["key"] = "value" };
            var wrapper = new DictionaryWrapper(dict, new Dictionary<string, Func<object>>());
            var item = new KeyValuePair<string, object>("key", "wrongValue");

            // Act & Assert
            Assert.DoesNotContain(item, wrapper);
        }

        [Fact]
        public void ContainsKeyValuePair_ShouldReturnFalseForMissingKey()
        {
            // Arrange
            var wrapper = new DictionaryWrapper(new Dictionary<string, object>(), new Dictionary<string, Func<object>>());
            var item = new KeyValuePair<string, object>("missing", "value");

            // Act & Assert
            Assert.DoesNotContain(item, wrapper);
        }

        [Fact]
        public void RemoveKeyValuePair_ShouldReturnTrueAndRemove()
        {
            // Arrange
            var dict = new Dictionary<string, object> { ["key"] = "value" };
            var wrapper = new DictionaryWrapper(dict, new Dictionary<string, Func<object>>());
            var item = new KeyValuePair<string, object>("key", "value");

            // Act
            var result = wrapper.Remove(item);

            // Assert
            Assert.True(result);
            Assert.Empty(dict);
        }

        [Fact]
        public void RemoveKeyValuePair_ShouldThrowForBehaviorKey()
        {
            // Arrange
            var behavior = new Dictionary<string, Func<object>> { ["reserved"] = () => "null" };
            var wrapper = new DictionaryWrapper(new Dictionary<string, object>(), behavior);
            var item = new KeyValuePair<string, object>("reserved", "value");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => wrapper.Remove(item));
        }

        [Fact]
        public void IEnumerableGetEnumerator_ShouldHandleEmptyCollections()
        {
            // Arrange
            var wrapper = new DictionaryWrapper(
                new Dictionary<string, object>(),
                new Dictionary<string, Func<object>>());

            // Act
            var count = Enumerable.Range(0, int.MaxValue)
            .TakeWhile(_ => ((IEnumerable)wrapper).GetEnumerator().MoveNext())
            .Count();

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void TryGetValue_ShouldReturnBaseValue_WhenKeyOnlyInBaseDictionary()
        {
            // Arrange
            var baseDict = new Dictionary<string, object> { ["baseOnly"] = "base_value" };
            var behavior = new Dictionary<string, Func<object>>();
            var wrapper = new DictionaryWrapper(baseDict, behavior);

            // Act
            var result = wrapper.TryGetValue("baseOnly", out var value);

            // Assert
            Assert.True(result);
            Assert.Equal("base_value", value);
        }
    }
}