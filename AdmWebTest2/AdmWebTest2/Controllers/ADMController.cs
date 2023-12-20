using admtest5.Id3Classes;
using AdmWebTest2.Models;
using AdmWebTest2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace AdmWebTest2.Controllers
{
    public class ADMController : Controller
    {
       public static double pi = Math.Acos(-1.0);
            public static DB_A620E5_heartDatasetEntities _context = new DB_A620E5_heartDatasetEntities();
            public static List<string> columnsName = new List<string>();
            public static List<Type> columnsType = new List<Type>();
            public static string columnsTarget;
            public static int columnsNumber;
            public static List<IQueryable> columnsDistValue = new List<IQueryable>();
            static Dictionary<string, Attribut> Attributes = new Dictionary<string, Attribut>();
            public static Dictionary<string, Type> AttributesType = new Dictionary<string, Type>();
            static Dictionary<Attribut, bool> defaultParentAttrib = new Dictionary<Attribut, bool>();
            public static Dictionary<string, Dictionary<string, List<int>>> bayesAnswer = new Dictionary<string, Dictionary<string, List<int>>>();
            public static Dictionary<string, List<Tuple<double, double>>> bayesNumbersColumns = new Dictionary<string, List<Tuple<double, double>>>();
            static TreeNode ansTree;
            public static int leafsNumber = 0;
            public static int get_type(int index)
            {

                if (columnsType[index].Name == TypeCode.Byte.ToString()
                   || columnsType[index].Name == TypeCode.Decimal.ToString()
                   || columnsType[index].Name == TypeCode.Double.ToString()
                   || columnsType[index].Name == TypeCode.Int16.ToString()
                   || columnsType[index].Name == TypeCode.Int32.ToString()
                   || columnsType[index].Name == TypeCode.Int64.ToString()
                   || columnsType[index].Name == TypeCode.SByte.ToString()
                   || columnsType[index].Name == TypeCode.Single.ToString()
                   || columnsType[index].Name == TypeCode.UInt16.ToString()
                   || columnsType[index].Name == TypeCode.UInt32.ToString()
                   || columnsType[index].Name == TypeCode.UInt64.ToString())
                    return -1;
                else if (columnsType[index].Name == TypeCode.Char.ToString()
                        || columnsType[index].Name == TypeCode.String.ToString())
                    return 1;

                return 0;
            }

            public static List<string> get_children(int index)
            {
                List<string> children = new List<string>();
                int type = get_type(index);
                if (type == 1)
                {

                    foreach (var item in columnsDistValue[index])
                    {
                        if (item.ToString() != "?")
                            children.Add("\"" + item.ToString() + "\"");
                    }
                }
                else if (type == 0)
                {
                    foreach (var item in columnsDistValue[index])
                    {
                        if (item.ToString() != "?")
                            children.Add(item.ToString());
                    }
                }
                else if (type == -1)
                {
                    int start = 1000000000, end = -1000000000;
                    foreach (var item in columnsDistValue[index])
                    {
                        int x = Int32.Parse(item.ToString());
                        if (start >= x)
                            start = x;
                        if (end <= x)
                            end = x;
                    }
                    int temprange = (end - start + 4) / 4;
                    start += temprange;
                    while (start < end)
                    {
                        children.Add((start - temprange).ToString() + " And " + start.ToString());
                        start += temprange;
                    }
                    children.Add((start - temprange).ToString() + " And " + start.ToString());
                }
                else
                {
                    foreach (var item in columnsDistValue[index])
                    {
                        children.Add(item.ToString());
                    }
                }
                return children;
            }
            static void setup()
            {

                columnsTarget = "disease";
                columnsName = typeof(C_heart_dataset__).GetProperties()
                           .Where(property => property.Name != "id")
                           .Select(property => property.Name)
                           .ToList();
                columnsType = typeof(C_heart_dataset__).GetProperties()
                            .Where(property => property.Name != "id")
                            .Select(property => property.PropertyType)
                            .ToList();
                columnsNumber = columnsName.Count;
                for (int i = 0; i < columnsNumber; i++)
                {
                    columnsDistValue.Add(_context.C_heart_dataset__.Select(columnsName[i]).Distinct());

                }
                columnsNumber--;
                for (int i = 0; i < columnsNumber; i++)
                {

                    Attributes.Add(columnsName[i], new Attribut(columnsName[i], get_children(i), get_type(i), null));
                }
                foreach (var item in columnsDistValue[columnsNumber])
                {
                    Attributes.Add(item.ToString(), new Attribut(item.ToString(), null, get_type(columnsNumber), null));
                }
                foreach (var item in Attributes)
                {
                    if (item.Value.dist_attribut_value != null)
                        defaultParentAttrib.Add(item.Value, false);
                }
                Attributes.Add("missing", new Attribut("missing", null, 1, null));
            }
            static Dictionary<Attribut, bool> getIMageOfParentAttrib(Dictionary<Attribut, bool> input)
            {
                Dictionary<Attribut, bool> output = new Dictionary<Attribut, bool>();
                foreach (var item in input)
                {
                    output.Add(item.Key, item.Value);
                }
                return output;
            }
            static string getAnd(string condition)
            {
                if (condition.Length > 0)
                    return " And ";
                else
                    return "";
            }
            static List<int> getValuesForNode(Attribut attrib, int child_index, string condition)
            {
                List<int> ans = new List<int>();
                int sum = 0;
                string quary = condition;

                if (attrib != null)
                    if (attrib.type == -1)
                        quary += getAnd(condition) + attrib.name + " >= " + attrib.dist_attribut_value[child_index] + " > " + attrib.name;
                    else
                        quary += getAnd(condition) + attrib.name + " = " + attrib.dist_attribut_value[child_index];
                foreach (var item in columnsDistValue[columnsNumber])
                {
                    string param = "\"" + item.ToString() + "\"";
                    ans.Add(_context.C_heart_dataset__.Where(quary + getAnd(quary) + columnsTarget + " = " + param).Select(columnsTarget).Count());
                    sum += ans[ans.Count - 1];
                }
                ans.Add(sum);
                return ans;
            }
            static double getTotalSetCountForNode(string condition)
            {

                double ans = 0;
                if (condition.Length > 0)
                    ans = _context.C_heart_dataset__.Where(condition).Select(columnsTarget).Count();
                else
                    ans = _context.C_heart_dataset__.Select(columnsTarget).Count();
                return ans;
            }

            static double calcEntropy(List<int> pos_neg, double total)
            {
                double ans = 0;
                for (int i = 0; i < pos_neg.Count - 1; i++)
                {
                    if (pos_neg[i] != 0)
                        ans += pos_neg[i] / total * Math.Log(pos_neg[i] / total, 2);
                }
                return -1 * ans;
            }

            static double iAttrib(Attribut attrib, string condition)
            {

                double sum = 0.0;
                int coun = 0;
                double total = getTotalSetCountForNode(condition);
                for (int i = 0; i < attrib.dist_attribut_value.Count; i++)
                {
                    List<int> result_values = getValuesForNode(attrib, i, condition);
                    if (result_values[result_values.Count - 1] == 0)
                        coun++;
                    sum += result_values[result_values.Count - 1] / total * calcEntropy(result_values, result_values[result_values.Count - 1]);
                }
                if (attrib.type == -1 && coun > 1)
                    return 10;
                return sum;
            }

            static string gain(Dictionary<Attribut, bool> is_parent, string condition)
            {
                double max_gain = 0;
                string ans = "";
                List<int> getvalue = getValuesForNode(null, -1, condition);
                int p_n = 0;

                foreach (var item in columnsDistValue[columnsNumber])
                {
                    if (getvalue[p_n++] == getvalue[getvalue.Count - 1] && getvalue[getvalue.Count - 1] != 0)
                        ans = item.ToString();
                }
                if (ans != "")
                    return ans;
                double entropy_s = calcEntropy(getvalue, getvalue[getvalue.Count - 1]);
                foreach (var item in is_parent)
                {
                    double gain = 0;
                    if (!item.Value)
                    {
                        gain = entropy_s - iAttrib(item.Key, condition);
                        if (max_gain < gain)
                        {
                            max_gain = gain;
                            ans = item.Key.name;
                        }
                    }

                }

                return ans;
            }
            static TreeNode getTreeAnswer(TreeNode parent)
            {
                TreeNode node = null;

                if (parent == null)
                {
                    Dictionary<Attribut, bool> parentImage = getIMageOfParentAttrib(defaultParentAttrib);
                    Attribut nodeAttribut = Attributes[gain(parentImage, "")];
                    TreeNode prenode = new TreeNode(nodeAttribut, "", parentImage);
                    node = getTreeAnswer(prenode);
                    return node;
                }
                else
                {

                    if (parent.node_attribute == null || parent.node_attribute.name == null || parent.node_attribute.name == "missing")
                    {
                        return parent;
                    }
                    foreach (var item in columnsDistValue[columnsNumber])
                        if (parent.node_attribute.name == item.ToString())
                        {
                            leafsNumber++;
                            return parent;
                        }
                    Dictionary<Attribut, bool> parentImage = getIMageOfParentAttrib(parent.is_parent);
                    parentImage[parent.node_attribute] = true;
                    for (int i = 0; i < parent.node_attribute.dist_attribut_value.Count; i++)
                    {
                        string condition = parent.condition;
                        if (parent.node_attribute.type == -1)
                            condition += getAnd(parent.condition) + parent.node_attribute.name + " >= " + parent.node_attribute.dist_attribut_value[i] + " > " + parent.node_attribute.name;
                        else
                            condition += getAnd(parent.condition) + parent.node_attribute.name + " = " + parent.node_attribute.dist_attribut_value[i];
                        string attribName = gain(parentImage, condition);
                        Attribut nodeAttribut;
                        if (attribName != "")
                            nodeAttribut = Attributes[attribName];
                        else
                            nodeAttribut = null;
                        TreeNode prenode = new TreeNode(nodeAttribut, condition, parentImage);
                        TreeNode prenodeTree = getTreeAnswer(prenode);
                        if (prenodeTree != null)
                            prenode.childern = prenodeTree.childern;
                        parent.childern.Add(prenode);

                    }
                    return parent;
                }

            }
            static TreeNode startID3()
            {

                TreeNode ansTree = getTreeAnswer(null);
                return ansTree;
            }
            static Tuple<int, int> getNumbersOfTheCondition(string str)
            {
                int v1 = 0, v2 = 0;
                string st1 = "", temp = "", st2 = "";
                for (int i = 0; str[i] != ' '; i++)
                    st1 += str[i];
                for (int i = str.Length - 1; str[i] != ' '; i--)
                    temp += str[i];
                for (int i = temp.Length - 1; i >= 0; i--)
                    st2 += temp;

                v1 = Int32.Parse(st1);
                v2 = Int32.Parse(st2);
                return new Tuple<int, int>(v1, v2);
            }
            static void initNumicColumnsForBayes(int index)
            {
                bayesNumbersColumns.Add(columnsName[index], new List<Tuple<double, double>>());
                foreach (var item in columnsDistValue[columnsNumber])
                {
                    string condition = columnsTarget + " = " + "\"" + item.ToString() + "\"";
                    var count = _context.C_heart_dataset__.Where(condition).Count();
                    int sum = 0;
                    var listOfNumers = _context.C_heart_dataset__.Where(condition).Select(columnsName[index]);
                    foreach (var x in listOfNumers)
                    {
                        sum += Int32.Parse(x.ToString());
                    }
                    double mean = sum * 1.0 / listOfNumers.Count();
                    var sigma = 0.0;
                    foreach (var x in listOfNumers)
                    {
                        int num = Int32.Parse(x.ToString());
                        sigma += (num - mean) * (num - mean);
                    }
                    sigma /= (count - 1);
                    Tuple<double, double> t = new Tuple<double, double>(mean, sigma);
                    bayesNumbersColumns[columnsName[index]].Add(t);
                }
            }
            static void startBayes()
            {
                for (int i = 0; i <= columnsNumber; i++)
                {
                    int typeValue = get_type(i);
                    if (typeValue != -1)
                    {
                        bayesAnswer.Add(columnsName[i], new Dictionary<string, List<int>>());
                        foreach (var item in columnsDistValue[i])
                        {

                            string condition = "";
                            if (typeValue == 1)
                                condition = columnsName[i] + " = " + "\"" + item.ToString() + "\"";
                            else
                                condition = columnsName[i] + " = " + item.ToString();
                            var l = getValuesForNode(null, -1, condition);
                            bayesAnswer[columnsName[i]].Add(item.ToString(), l);

                        }
                    }
                    else
                    {
                        initNumicColumnsForBayes(i);
                    }
                }

            }
            static double Gaussian(Tuple<double, double> numValues, int input)
            {
                double ans = 1 / Math.Sqrt(2 * pi * numValues.Item2) * Math.Exp(-1 * ((input - numValues.Item1) * (input - numValues.Item1)) / (2 * numValues.Item2));
                return ans;
            }
            static string testID3(List<string> input, TreeNode node)
            {
                if (node.node_attribute == null)
                    return "missing";
                if (node.childern.Count == 0)
                    return node.node_attribute.name;
                string ans = null;
                for (int i = 0; i < columnsNumber; i++)
                {

                    if (columnsName[i] == node.node_attribute.name)
                    {
                        for (int j = 0; j < node.node_attribute.dist_attribut_value.Count; j++)
                        {
                            if (get_type(i) == -1)
                            {
                                Tuple<int, int> resOfValueInInteger = getNumbersOfTheCondition(node.node_attribute.dist_attribut_value[j]);
                                int inputnum = Int32.Parse(input[i]);
                                if (inputnum >= resOfValueInInteger.Item1 && inputnum < resOfValueInInteger.Item2)
                                    ans = testID3(input, node.childern[j]);
                            }
                            else
                            {
                                if (input[i] == node.node_attribute.dist_attribut_value[j])
                                    ans = testID3(input, node.childern[j]);
                            }
                        }
                    }

                }
                return ans;
            }
            static Tuple<string, double> testBayes(List<string> input)
            {
                List<double> ans = new List<double>();
                int j = 0;
                double sum = 0;
                foreach (var item in columnsDistValue[columnsNumber])
                {
                    double tempans = 1;
                    for (int i = 0; i < columnsNumber; i++)
                    {
                        int type = get_type(i);
                        if (type == -1)
                        {
                            tempans *= Gaussian(bayesNumbersColumns[columnsName[i]][j], Int32.Parse(input[i]));
                        }
                        else
                        {
                            tempans *= bayesAnswer[columnsName[i]][input[i]][j] * 1.0 / bayesAnswer[columnsTarget][item.ToString()][j];
                        }
                    }
                    tempans *= bayesAnswer[columnsTarget][item.ToString()][j] * 1.0 / bayesAnswer[columnsTarget][item.ToString()][bayesAnswer[columnsTarget][item.ToString()].Count - 1];
                    j++;
                    sum += tempans;
                    ans.Add(tempans);
                }
                for (int i = 0; i < ans.Count; i++)
                {
                    ans[i] = ans[i] / sum;
                }
                double max_value = -1;
                int id = 0;
                string strans = null;
                int idx = 0;
                foreach (var item in columnsDistValue[columnsNumber])
                {
                    if (max_value < ans[idx])
                    {
                        max_value = ans[idx];
                        strans = item.ToString();
                        id = idx;
                    }
                    idx++;
                }
                return new Tuple<string, double>(strans, max_value);
            }
            public ActionResult SetupAlgorthems()
            {
                if (ansTree != null && Attributes.Count>0)
                    return View("Index");
                setup();
                ansTree = startID3();
                startBayes();
                return View("Index");
            }
            [HttpPost]
            public ActionResult testID3Algorthem(C_heart_dataset__ x)
            {
                    if(!ModelState.IsValid)
                    {
                         return View("Index", x);
                    }
                    List<string> input = new List<string>()
                    {
                        x.age.ToString(),
                        x.chest_pain_type,
                        x.rest_blood_pressure.ToString(),
                        x.blood_sugar.ToString(),
                        x.rest_electro,
                        x.max_heart_rate.ToString(),
                        x.exercice_angina
                    };
                   
                    var ans = testID3(input, ansTree);
                     if (ans == "missing")
                            ans = "negative";
                    var z = new result();
                     z.id3Output = ans;
                     return View("res",z);
            }

             public ActionResult testBayesAlgorthem(C_heart_dataset__ x)
             {
                if (!ModelState.IsValid)
                {
                    return View("Index", x);
                }
                List<string> input = new List<string>()
                            {
                                x.age.ToString(),
                                x.chest_pain_type,
                                x.rest_blood_pressure.ToString(),
                                x.blood_sugar.ToString(),
                                x.rest_electro,
                                x.max_heart_rate.ToString(),
                                x.exercice_angina
                            };
                   var ans = testBayes(input);
                var z = new result();
                z.bayesOutput = ans;
                return View("res", z);
             }
        }

    }


