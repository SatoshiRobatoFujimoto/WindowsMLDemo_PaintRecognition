#if UNITY_UWP
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// number

namespace number
{
    public sealed class NumberModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class NumberModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public NumberModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>();
            loss.Add("0", float.NaN);
            loss.Add("1", float.NaN);
            loss.Add("2", float.NaN);
            loss.Add("3", float.NaN);
            loss.Add("4", float.NaN);
            loss.Add("5", float.NaN);
            loss.Add("6", float.NaN);
            loss.Add("7", float.NaN);
            loss.Add("8", float.NaN);
            loss.Add("9", float.NaN);
        }
    }

    public sealed class NumberModel
    {
        private LearningModelPreview learningModel;
        public static async Task<NumberModel> CreateNumberModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            NumberModel model = new NumberModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<NumberModelOutput> EvaluateAsync(NumberModelInput input) {
            NumberModelOutput output = new NumberModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
#endif
