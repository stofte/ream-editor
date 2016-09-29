import { AutocompletionQuery } from '../models/index';

export const cSharpDatabaseCodeCheckExpectedErrors = [
{ text: 'The name \'FOO\' does not exist in the current context',
       logLevel: 'Error',
       line: 0,
       column: 0,
       endLine: 0,
       endColumn: 3 }
];

export const cSharpDatabaseCodeCheckEditorTestData = [
{
    "output": "city.Take(10).Select(x => x.Name);\n",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["F"],"removed":[""],"origin":"+input","time":0},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["O"],"removed":[""],"origin":"+input","time":231.64499999999998},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["O"],"removed":[""],"origin":"+input","time":408.5750000000003},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":3},"text":[""],"removed":["O"],"origin":"+delete","time":8790.740000000002},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":2},"text":[""],"removed":["O"],"origin":"+delete","time":9042.735000000002},
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":1},"text":[""],"removed":["F"],"origin":"+delete","time":9263.655000000002},
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["c"],"removed":[""],"origin":"+input","time":10421.635000000002},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["i"],"removed":[""],"origin":"+input","time":10564.810000000001},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["t"],"removed":[""],"origin":"+input","time":10689.900000000001},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":["y"],"removed":[""],"origin":"+input","time":10887.23},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["."],"removed":[""],"origin":"+input","time":11239.530000000002},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":["T"],"removed":[""],"origin":"+input","time":11431.980000000001},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["a"],"removed":[""],"origin":"+input","time":11637.18},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["k"],"removed":[""],"origin":"+input","time":11748.265000000001},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["e"],"removed":[""],"origin":"+input","time":11855.005000000001},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["("],"removed":[""],"origin":"+input","time":12095.100000000002},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":["1"],"removed":[""],"origin":"+input","time":12382.835000000001},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["0"],"removed":[""],"origin":"+input","time":12537.185000000001},
        {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":[")"],"removed":[""],"origin":"+input","time":12877.335000000003},
        {"from":{"line":0,"ch":13},"to":{"line":0,"ch":13},"text":["."],"removed":[""],"origin":"+input","time":13270.785000000002},
        {"from":{"line":0,"ch":14},"to":{"line":0,"ch":14},"text":["S"],"removed":[""],"origin":"+input","time":13536.880000000001},
        {"from":{"line":0,"ch":15},"to":{"line":0,"ch":15},"text":["e"],"removed":[""],"origin":"+input","time":13790.33},
        {"from":{"line":0,"ch":16},"to":{"line":0,"ch":16},"text":["l"],"removed":[""],"origin":"+input","time":13893.960000000001},
        {"from":{"line":0,"ch":17},"to":{"line":0,"ch":17},"text":["e"],"removed":[""],"origin":"+input","time":13993.77},
        {"from":{"line":0,"ch":18},"to":{"line":0,"ch":18},"text":["c"],"removed":[""],"origin":"+input","time":14206.94},
        {"from":{"line":0,"ch":19},"to":{"line":0,"ch":19},"text":["t"],"removed":[""],"origin":"+input","time":14421.315000000004},
        {"from":{"line":0,"ch":20},"to":{"line":0,"ch":20},"text":["("],"removed":[""],"origin":"+input","time":14681.68},
        {"from":{"line":0,"ch":21},"to":{"line":0,"ch":21},"text":["x"],"removed":[""],"origin":"+input","time":14867.740000000002},
        {"from":{"line":0,"ch":22},"to":{"line":0,"ch":22},"text":[" "],"removed":[""],"origin":"+input","time":15020.980000000003},
        {"from":{"line":0,"ch":23},"to":{"line":0,"ch":23},"text":["="],"removed":[""],"origin":"+input","time":15256.720000000001},
        {"from":{"line":0,"ch":24},"to":{"line":0,"ch":24},"text":[">"],"removed":[""],"origin":"+input","time":15373.84},
        {"from":{"line":0,"ch":25},"to":{"line":0,"ch":25},"text":[" "],"removed":[""],"origin":"+input","time":15603.915},
        {"from":{"line":0,"ch":26},"to":{"line":0,"ch":26},"text":["x"],"removed":[""],"origin":"+input","time":15821.575000000004},
        {"from":{"line":0,"ch":27},"to":{"line":0,"ch":27},"text":["."],"removed":[""],"origin":"+input","time":16100.54},
        {"from":{"line":0,"ch":28},"to":{"line":0,"ch":28},"text":["N"],"removed":[""],"origin":"+input","time":16434.345},
        {"from":{"line":0,"ch":29},"to":{"line":0,"ch":29},"text":["a"],"removed":[""],"origin":"+input","time":16627.025},
        {"from":{"line":0,"ch":30},"to":{"line":0,"ch":30},"text":["m"],"removed":[""],"origin":"+input","time":16738.06},
        {"from":{"line":0,"ch":31},"to":{"line":0,"ch":31},"text":["e"],"removed":[""],"origin":"+input","time":16854.86},
        {"from":{"line":0,"ch":32},"to":{"line":0,"ch":32},"text":[")"],"removed":[""],"origin":"+input","time":17076.975000000002},
        {"from":{"line":0,"ch":33},"to":{"line":0,"ch":33},"text":[";"],"removed":[""],"origin":"+input","time":17466.08},
        {"from":{"line":0,"ch":34},"to":{"line":0,"ch":34},"text":["",""],"removed":[""],"origin":"+input","time":18206.270000000004}
]}
];

export const cSharpCityFilteringQueryEditorTestData = [
{
    "output": "from c in this.city\nwhere c.Name.StartsWith(\"Ca\")\nselect c\n",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["f"],"removed":[""],"origin":"+input","time":0},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["r"],"removed":[""],"origin":"+input","time":110.22500000000002},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["o"],"removed":[""],"origin":"+input","time":221.12},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":["m"],"removed":[""],"origin":"+input","time":322.84000000000003},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":[" "],"removed":[""],"origin":"+input","time":552.995},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":["c"],"removed":[""],"origin":"+input","time":645.14},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":[" "],"removed":[""],"origin":"+input","time":892.3000000000001},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["n"],"removed":[""],"origin":"+input","time":1135.2050000000004},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":8},"text":[""],"removed":["n"],"origin":"+delete","time":1556.7050000000004},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["i"],"removed":[""],"origin":"+input","time":1768.9650000000006},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["n"],"removed":[""],"origin":"+input","time":1854.9100000000003},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":[" "],"removed":[""],"origin":"+input","time":2077.3000000000006},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":["t"],"removed":[""],"origin":"+input","time":2263.4750000000004},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["h"],"removed":[""],"origin":"+input","time":2326.335},
        {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":["i"],"removed":[""],"origin":"+input","time":2445.21},
        {"from":{"line":0,"ch":13},"to":{"line":0,"ch":13},"text":["s"],"removed":[""],"origin":"+input","time":2560.9000000000005},
        {"from":{"line":0,"ch":14},"to":{"line":0,"ch":14},"text":["."],"removed":[""],"origin":"+input","time":2776.925},
        {"from":{"line":0,"ch":15},"to":{"line":0,"ch":15},"text":["c"],"removed":[""],"origin":"+input","time":2948.695},
        {"from":{"line":0,"ch":16},"to":{"line":0,"ch":16},"text":["i"],"removed":[""],"origin":"+input","time":3075.415000000001},
        {"from":{"line":0,"ch":17},"to":{"line":0,"ch":17},"text":["t"],"removed":[""],"origin":"+input","time":3223.9300000000003},
        {"from":{"line":0,"ch":18},"to":{"line":0,"ch":18},"text":["y"],"removed":[""],"origin":"+input","time":3398.255},
        {"from":{"line":0,"ch":19},"to":{"line":0,"ch":19},"text":["",""],"removed":[""],"origin":"+input","time":3867.3950000000004},
        {"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["w"],"removed":[""],"origin":"+input","time":14330.575000000003},
        {"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["h"],"removed":[""],"origin":"+input","time":14545.685000000001},
        {"from":{"line":1,"ch":2},"to":{"line":1,"ch":2},"text":["e"],"removed":[""],"origin":"+input","time":14661.885000000002},
        {"from":{"line":1,"ch":3},"to":{"line":1,"ch":3},"text":["r"],"removed":[""],"origin":"+input","time":14758.37},
        {"from":{"line":1,"ch":4},"to":{"line":1,"ch":4},"text":["e"],"removed":[""],"origin":"+input","time":14899.625},
        {"from":{"line":1,"ch":5},"to":{"line":1,"ch":5},"text":[" "],"removed":[""],"origin":"+input","time":15114.900000000003},
        {"from":{"line":1,"ch":6},"to":{"line":1,"ch":6},"text":["c"],"removed":[""],"origin":"+input","time":17198.420000000002},
        {"from":{"line":1,"ch":7},"to":{"line":1,"ch":7},"text":["."],"removed":[""],"origin":"+input","time":17357.835000000003},
        {"from":{"line":1,"ch":8},"to":{"line":1,"ch":8},"text":["N"],"removed":[""],"origin":"+input","time":17689.75},
        {"from":{"line":1,"ch":9},"to":{"line":1,"ch":9},"text":["a"],"removed":[""],"origin":"+input","time":17884.74},
        {"from":{"line":1,"ch":10},"to":{"line":1,"ch":10},"text":["m"],"removed":[""],"origin":"+input","time":17995.825},
        {"from":{"line":1,"ch":11},"to":{"line":1,"ch":11},"text":["e"],"removed":[""],"origin":"+input","time":18112.97},
        {"from":{"line":1,"ch":12},"to":{"line":1,"ch":12},"text":["."],"removed":[""],"origin":"+input","time":18310.845},
        {"from":{"line":1,"ch":13},"to":{"line":1,"ch":13},"text":["S"],"removed":[""],"origin":"+input","time":18512.81},
        {"from":{"line":1,"ch":14},"to":{"line":1,"ch":14},"text":["t"],"removed":[""],"origin":"+input","time":18703.265000000003},
        {"from":{"line":1,"ch":15},"to":{"line":1,"ch":15},"text":["a"],"removed":[""],"origin":"+input","time":18883.475000000002},
        {"from":{"line":1,"ch":16},"to":{"line":1,"ch":16},"text":["r"],"removed":[""],"origin":"+input","time":19081.805000000004},
        {"from":{"line":1,"ch":17},"to":{"line":1,"ch":17},"text":["t"],"removed":[""],"origin":"+input","time":19383.41},
        {"from":{"line":1,"ch":18},"to":{"line":1,"ch":18},"text":["s"],"removed":[""],"origin":"+input","time":19611.085000000003},
        {"from":{"line":1,"ch":19},"to":{"line":1,"ch":19},"text":["W"],"removed":[""],"origin":"+input","time":19911.66},
        {"from":{"line":1,"ch":20},"to":{"line":1,"ch":20},"text":["i"],"removed":[""],"origin":"+input","time":20037.89},
        {"from":{"line":1,"ch":21},"to":{"line":1,"ch":21},"text":["t"],"removed":[""],"origin":"+input","time":20170.945},
        {"from":{"line":1,"ch":22},"to":{"line":1,"ch":22},"text":["h"],"removed":[""],"origin":"+input","time":21141.010000000002},
        {"from":{"line":1,"ch":23},"to":{"line":1,"ch":23},"text":["("],"removed":[""],"origin":"+input","time":21504.78},
        {"from":{"line":1,"ch":24},"to":{"line":1,"ch":24},"text":["\""],"removed":[""],"origin":"+input","time":21652.370000000003},
        {"from":{"line":1,"ch":25},"to":{"line":1,"ch":25},"text":["C"],"removed":[""],"origin":"+input","time":22055.975000000002},
        {"from":{"line":1,"ch":26},"to":{"line":1,"ch":26},"text":["a"],"removed":[""],"origin":"+input","time":22376.915},
        {"from":{"line":1,"ch":27},"to":{"line":1,"ch":27},"text":["\""],"removed":[""],"origin":"+input","time":22677.825},
        {"from":{"line":1,"ch":28},"to":{"line":1,"ch":28},"text":[")"],"removed":[""],"origin":"+input","time":22950.82},
        {"from":{"line":1,"ch":29},"to":{"line":1,"ch":29},"text":["",""],"removed":[""],"origin":"+input","time":23505.505000000005},
        {"from":{"line":2,"ch":0},"to":{"line":2,"ch":0},"text":["s"],"removed":[""],"origin":"+input","time":24370.910000000003},
        {"from":{"line":2,"ch":1},"to":{"line":2,"ch":1},"text":["e"],"removed":[""],"origin":"+input","time":24584.030000000002},
        {"from":{"line":2,"ch":2},"to":{"line":2,"ch":2},"text":["l"],"removed":[""],"origin":"+input","time":24687.9},
        {"from":{"line":2,"ch":3},"to":{"line":2,"ch":3},"text":["e"],"removed":[""],"origin":"+input","time":24781.02},
        {"from":{"line":2,"ch":4},"to":{"line":2,"ch":4},"text":["c"],"removed":[""],"origin":"+input","time":24984.31},
        {"from":{"line":2,"ch":5},"to":{"line":2,"ch":5},"text":["t"],"removed":[""],"origin":"+input","time":25216.760000000002},
        {"from":{"line":2,"ch":6},"to":{"line":2,"ch":6},"text":[" "],"removed":[""],"origin":"+input","time":25391.4},
        {"from":{"line":2,"ch":7},"to":{"line":2,"ch":7},"text":["c"],"removed":[""],"origin":"+input","time":26250.935},
        {"from":{"line":2,"ch":8},"to":{"line":2,"ch":8},"text":["",""],"removed":[""],"origin":"+input","time":26721.585000000003}
]}
];

export const cSharpContextSwitchExpectedCodeChecks = [
{ text: 'The name \'city\' does not exist in the current context',
       logLevel: 'Error',
       fileName: 'C:\\Users\\svto\\AppData\\Local\\ReamEditor\\omnisharp\\ba07f240f726b4d3d838bd09ad5003cc4.cs',
       line: 0,
       column: 0,
       endLine: 0,
       endColumn: 4 }];

export const cSharpContextSwitchEditorTestData = [
{
    "output": "city.Take(10)",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["c"],"removed":[""],"origin":"+input","time":0},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["i"],"removed":[""],"origin":"+input","time":256.43499999999995},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["t"],"removed":[""],"origin":"+input","time":489.845},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":["y"],"removed":[""],"origin":"+input","time":752.2250000000001},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["."],"removed":[""],"origin":"+input","time":5510.365000000001},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":["T"],"removed":[""],"origin":"+input","time":6271.055},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["a"],"removed":[""],"origin":"+input","time":6475.030000000001},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["k"],"removed":[""],"origin":"+input","time":6577.755},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["e"],"removed":[""],"origin":"+input","time":6717.830000000001},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["("],"removed":[""],"origin":"+input","time":6995.645000000001},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":["1"],"removed":[""],"origin":"+input","time":7364.795000000001},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["0"],"removed":[""],"origin":"+input","time":7533.715000000001},
        {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":[")"],"removed":[""],"origin":"+input","time":7840.990000000001}
]}
];

export const cSharpAutocompletionExpectedValues = [
    // expected items in the completion list on accessing an int instance variable
    [
        "CompareTo",
        "Equals",
        "GetHashCode",
        "ToString"
    ]
]

export const cSharpAutocompletionRequestTestData: AutocompletionQuery[] = [
    <AutocompletionQuery> {
        column: 2,
        line: 1,
        wantKind: true,
        wantDocumentationForEveryCompletionResult: true,
        wordToComplete: null,
        wantReturnType: true,
        wantMethodHeader: true
    }
];

export const cSharpAutocompletionEditorTestData = [
{
    "output": "int x = 42;\nx.",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["i"],"removed":[""],"origin":"+input","time":0},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["n"],"removed":[""],"origin":"+input","time":195.30999999999995},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["t"],"removed":[""],"origin":"+input","time":274.905},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":[" "],"removed":[""],"origin":"+input","time":657.4850000000001},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["x"],"removed":[""],"origin":"+input","time":843.2900000000002},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":[" "],"removed":[""],"origin":"+input","time":1020.3},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["="],"removed":[""],"origin":"+input","time":1311.6850000000002},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":[" "],"removed":[""],"origin":"+input","time":1405.2100000000003},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["4"],"removed":[""],"origin":"+input","time":1751.5650000000003},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["2"],"removed":[""],"origin":"+input","time":1781.305},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":[";"],"removed":[""],"origin":"+input","time":2139.1500000000005},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["",""],"removed":[""],"origin":"+input","time":2439.0699999999997},
        {"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["x"],"removed":[""],"origin":"+input","time":2721.0150000000003},
        {"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["."],"removed":[""],"origin":"+input","time":3081.24}
]}
];

export const cSharpTestDataExpectedResult = [
    [{
        title: 'int',
        rows: [42],
        columns: ['int'],
        columnTypes: ['System.Int32'],
    }],
    [{
        title: 'string',
        rows: ['foobar'],
        columns: ['string'],
        columnTypes: ['System.String'],
    }]
];

export const cSharpTestDataExpectedCodeChecks = [
{
    text: 'Invalid expression term \'}\'',
    logLevel: 'Error',
    line: 0,
    column: 8,
    endLine: 0,
    endColumn: 9 
},
{
    text: 'The variable \'x\' is assigned but its value is never used',
    logLevel: 'Warning',
    line: 0,
    column: 4,
    endLine: 0,
    endColumn: 5
}];

export const codecheckEditorTestData = [
{
    // we should see an error while paused after typing the "=" (~5 second gap), and none the second pause
    "output": "int x = 42;\n",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["i"],"removed":[""],"origin":"+input","time":0.005000000000563887},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["n"],"removed":[""],"origin":"+input","time":69.20000000000073},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["t"],"removed":[""],"origin":"+input","time":186.01500000000033},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":[" "],"removed":[""],"origin":"+input","time":336.37000000000035},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["x"],"removed":[""],"origin":"+input","time":457.37500000000045},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":[" "],"removed":[""],"origin":"+input","time":659.4050000000007},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["="],"removed":[""],"origin":"+input","time":870.7500000000005},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":[" "],"removed":[""],"origin":"+input","time":979.4500000000007},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["4"],"removed":[""],"origin":"+input","time":6001.935},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["2"],"removed":[""],"origin":"+input","time":6082.325000000001},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":[";"],"removed":[""],"origin":"+input","time":6217.8150000000005},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["",""],"removed":[""],"origin":"+input","time":8646.215}
]}
];

export const cSharpTestData = [
    {"output":"var x = 21;\nx*2\n","events":[{"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["v"],"removed":[""],"origin":"+input","time":0},{"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["a"],"removed":[""],"origin":"+input","time":281.0450000000001},{"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["r"],"removed":[""],"origin":"+input","time":368.4699999999998},{"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":[" "],"removed":[""],"origin":"+input","time":496.09500000000025},{"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["x"],"removed":[""],"origin":"+input","time":952.46},{"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":[" "],"removed":[""],"origin":"+input","time":1089.255},{"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["="],"removed":[""],"origin":"+input","time":1301.13},{"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":[" "],"removed":[""],"origin":"+input","time":1396.7000000000003},{"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["2"],"removed":[""],"origin":"+input","time":2226.0000000000005},{"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["1"],"removed":[""],"origin":"+input","time":2320.1700000000005},{"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":[";"],"removed":[""],"origin":"+input","time":2637.605},{"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["",""],"removed":[""],"origin":"+input","time":3154.0900000000006},{"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["x"],"removed":[""],"origin":"+input","time":3737.645},{"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["*"],"removed":[""],"origin":"+input","time":4653.720000000001},{"from":{"line":1,"ch":2},"to":{"line":1,"ch":2},"text":["2"],"removed":[""],"origin":"+input","time":5061.935000000001},{"from":{"line":1,"ch":3},"to":{"line":1,"ch":3},"text":["",""],"removed":[""],"origin":"+input","time":6090.575000000001}]},
{
    "output": "var someStr = \"foo\";\nsomeStr+\"bar\"",
    "events": [
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["v"],"removed":[""],"origin":"+input","time":0},
        {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["a"],"removed":[""],"origin":"+input","time":155.22000000000116},
        {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["r"],"removed":[""],"origin":"+input","time":241.0600000000013},
        {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":[" "],"removed":[""],"origin":"+input","time":511.46500000000196},
        {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["s"],"removed":[""],"origin":"+input","time":1049.2000000000007},
        {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":["o"],"removed":[""],"origin":"+input","time":1174.125},
        {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["m"],"removed":[""],"origin":"+input","time":1246.260000000002},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["S"],"removed":[""],"origin":"+input","time":1576.5000000000018},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":8},"text":[""],"removed":["S"],"origin":"+delete","time":2080.9000000000015},
        {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["e"],"removed":[""],"origin":"+input","time":2201.4000000000015},
        {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["S"],"removed":[""],"origin":"+input","time":2486.8999999999996},
        {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["t"],"removed":[""],"origin":"+input","time":2707.2699999999986},
        {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":["r"],"removed":[""],"origin":"+input","time":2784.6500000000033},
        {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":[" "],"removed":[""],"origin":"+input","time":2951.504999999999},
        {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":["="],"removed":[""],"origin":"+input","time":3150.01},
        {"from":{"line":0,"ch":13},"to":{"line":0,"ch":13},"text":[" "],"removed":[""],"origin":"+input","time":3275.074999999999},
        {"from":{"line":0,"ch":14},"to":{"line":0,"ch":14},"text":["\""],"removed":[""],"origin":"+input","time":3785.9200000000037},
        {"from":{"line":0,"ch":15},"to":{"line":0,"ch":15},"text":["f"],"removed":[""],"origin":"+input","time":4117.340000000002},
        {"from":{"line":0,"ch":16},"to":{"line":0,"ch":16},"text":["o"],"removed":[""],"origin":"+input","time":4277.175000000001},
        {"from":{"line":0,"ch":17},"to":{"line":0,"ch":17},"text":["o"],"removed":[""],"origin":"+input","time":4455.060000000003},
        {"from":{"line":0,"ch":18},"to":{"line":0,"ch":18},"text":["\""],"removed":[""],"origin":"+input","time":4667.845000000003},
        {"from":{"line":0,"ch":19},"to":{"line":0,"ch":19},"text":[";"],"removed":[""],"origin":"+input","time":4836.000000000002},
        {"from":{"line":0,"ch":20},"to":{"line":0,"ch":20},"text":["",""],"removed":[""],"origin":"+input","time":5416.890000000001},
        {"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["s"],"removed":[""],"origin":"+input","time":6526.6950000000015},
        {"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["o"],"removed":[""],"origin":"+input","time":6821.990000000003},
        {"from":{"line":1,"ch":2},"to":{"line":1,"ch":2},"text":["m"],"removed":[""],"origin":"+input","time":6923.630000000003},
        {"from":{"line":1,"ch":3},"to":{"line":1,"ch":3},"text":["e"],"removed":[""],"origin":"+input","time":7001.700000000003},
        {"from":{"line":1,"ch":4},"to":{"line":1,"ch":4},"text":["S"],"removed":[""],"origin":"+input","time":7267.360000000002},
        {"from":{"line":1,"ch":5},"to":{"line":1,"ch":5},"text":["t"],"removed":[""],"origin":"+input","time":7505.115000000003},
        {"from":{"line":1,"ch":6},"to":{"line":1,"ch":6},"text":["r"],"removed":[""],"origin":"+input","time":7567.435000000003},
        {"from":{"line":1,"ch":7},"to":{"line":1,"ch":7},"text":["+"],"removed":[""],"origin":"+input","time":8389.185000000003},
        {"from":{"line":1,"ch":8},"to":{"line":1,"ch":8},"text":["\""],"removed":[""],"origin":"+input","time":8790.19},
        {"from":{"line":1,"ch":9},"to":{"line":1,"ch":9},"text":["x"],"removed":[""],"origin":"+input","time":9399.400000000003},
        {"from":{"line":1,"ch":9},"to":{"line":1,"ch":10},"text":[""],"removed":["x"],"origin":"+delete","time":10252.210000000001},
        {"from":{"line":1,"ch":9},"to":{"line":1,"ch":9},"text":["b"],"removed":[""],"origin":"+input","time":10599.270000000002},
        {"from":{"line":1,"ch":10},"to":{"line":1,"ch":10},"text":["a"],"removed":[""],"origin":"+input","time":10749.500000000002},
        {"from":{"line":1,"ch":11},"to":{"line":1,"ch":11},"text":["r"],"removed":[""],"origin":"+input","time":10795.485000000002},
        {"from":{"line":1,"ch":12},"to":{"line":1,"ch":12},"text":["\""],"removed":[""],"origin":"+input","time":11142.230000000005}
]}
];

export const randomTestData = [
    {"output":"y","events":[
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["x"],"removed":[""],"origin":"+input","time":0.004999999999881766},
        {"from":{"line":0,"ch":0},"to":{"line":0,"ch":1},"text":["y"],"removed":["x"],"origin":"+input","time":2916.135}]
    },
    {
        "output": "test",
        "events": [
            {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["t"],"removed":[""],"origin":"+input","time":0.010000000000104592},
            {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["e"],"removed":[""],"origin":"+input","time":416.255},
            {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["s"],"removed":[""],"origin":"+input","time":842.745},
            {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":["t"],"removed":[""],"origin":"+input","time":1088.5600000000004},
            {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":["",""],"removed":[""],"origin":"+input","time":1312.5549999999998},
            {"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["h"],"removed":[""],"origin":"+input","time":1755.3899999999999},
            {"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["e"],"removed":[""],"origin":"+input","time":1913.1},
            {"from":{"line":1,"ch":2},"to":{"line":1,"ch":2},"text":["s"],"removed":[""],"origin":"+input","time":2084.945},
            {"from":{"line":1,"ch":3},"to":{"line":1,"ch":3},"text":["t"],"removed":[""],"origin":"+input","time":2179.05},
            {"from":{"line":0,"ch":2},"to":{"line":1,"ch":2,"xRel":0},"text":[""],"removed":["st","he"],"origin":"+delete","time":4920.77}
    ]},
    {
        "output": "some initial\ngoes here\n",
        "events": [
            {"from":{"line":0,"ch":0},"to":{"line":0,"ch":0},"text":["s"],"removed":[""],"origin":"+input","time":0},
            {"from":{"line":0,"ch":1},"to":{"line":0,"ch":1},"text":["o"],"removed":[""],"origin":"+input","time":164.34500000000003},
            {"from":{"line":0,"ch":2},"to":{"line":0,"ch":2},"text":["m"],"removed":[""],"origin":"+input","time":258.19499999999994},
            {"from":{"line":0,"ch":3},"to":{"line":0,"ch":3},"text":["e"],"removed":[""],"origin":"+input","time":397.655},
            {"from":{"line":0,"ch":4},"to":{"line":0,"ch":4},"text":[" "],"removed":[""],"origin":"+input","time":517.5499999999995},
            {"from":{"line":0,"ch":5},"to":{"line":0,"ch":5},"text":["i"],"removed":[""],"origin":"+input","time":665.4600000000003},
            {"from":{"line":0,"ch":6},"to":{"line":0,"ch":6},"text":["n"],"removed":[""],"origin":"+input","time":743.8299999999997},
            {"from":{"line":0,"ch":7},"to":{"line":0,"ch":7},"text":["i"],"removed":[""],"origin":"+input","time":892.7099999999998},
            {"from":{"line":0,"ch":8},"to":{"line":0,"ch":8},"text":["t"],"removed":[""],"origin":"+input","time":930.5350000000001},
            {"from":{"line":0,"ch":9},"to":{"line":0,"ch":9},"text":["i"],"removed":[""],"origin":"+input","time":1087.8849999999995},
            {"from":{"line":0,"ch":10},"to":{"line":0,"ch":10},"text":["a"],"removed":[""],"origin":"+input","time":1195.3500000000001},
            {"from":{"line":0,"ch":11},"to":{"line":0,"ch":11},"text":["l"],"removed":[""],"origin":"+input","time":1331.8749999999998},
            {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":[" "],"removed":[""],"origin":"+input","time":1490.4750000000001},
            {"from":{"line":0,"ch":13},"to":{"line":0,"ch":13},"text":["t"],"removed":[""],"origin":"+input","time":1616.265},
            {"from":{"line":0,"ch":13},"to":{"line":0,"ch":14},"text":[""],"removed":["t"],"origin":"+delete","time":2188.74},
            {"from":{"line":0,"ch":12},"to":{"line":0,"ch":13},"text":[""],"removed":[" "],"origin":"+delete","time":2377.505},
            {"from":{"line":0,"ch":12},"to":{"line":0,"ch":12},"text":["",""],"removed":[""],"origin":"+input","time":2719.4349999999995},
            {"from":{"line":1,"ch":0},"to":{"line":1,"ch":0},"text":["t"],"removed":[""],"origin":"+input","time":3089.55},
            {"from":{"line":1,"ch":1},"to":{"line":1,"ch":1},"text":["e"],"removed":[""],"origin":"+input","time":3167.135},
            {"from":{"line":1,"ch":2},"to":{"line":1,"ch":2},"text":["x"],"removed":[""],"origin":"+input","time":3410.99},
            {"from":{"line":1,"ch":3},"to":{"line":1,"ch":3},"text":["t"],"removed":[""],"origin":"+input","time":3602.9799999999996},
            {"from":{"line":1,"ch":4},"to":{"line":1,"ch":4},"text":["",""],"removed":[""],"origin":"+input","time":4017.455},
            {"from":{"line":2,"ch":0},"to":{"line":2,"ch":0},"text":["g"],"removed":[""],"origin":"+input","time":4323.57},
            {"from":{"line":2,"ch":1},"to":{"line":2,"ch":1},"text":["o"],"removed":[""],"origin":"+input","time":4426.685},
            {"from":{"line":2,"ch":2},"to":{"line":2,"ch":2},"text":["e"],"removed":[""],"origin":"+input","time":4661.775000000001},
            {"from":{"line":2,"ch":3},"to":{"line":2,"ch":3},"text":["s"],"removed":[""],"origin":"+input","time":4858.42},
            {"from":{"line":2,"ch":4},"to":{"line":2,"ch":4},"text":[" "],"removed":[""],"origin":"+input","time":4963.549999999999},
            {"from":{"line":2,"ch":5},"to":{"line":2,"ch":5},"text":["h"],"removed":[""],"origin":"+input","time":5095.055},
            {"from":{"line":2,"ch":6},"to":{"line":2,"ch":6},"text":["e"],"removed":[""],"origin":"+input","time":5180.665},
            {"from":{"line":2,"ch":7},"to":{"line":2,"ch":7},"text":["r"],"removed":[""],"origin":"+input","time":5274.735},
            {"from":{"line":2,"ch":8},"to":{"line":2,"ch":8},"text":["e"],"removed":[""],"origin":"+input","time":5368.845},
            {"from":{"line":2,"ch":9},"to":{"line":2,"ch":9},"text":["",""],"removed":[""],"origin":"+input","time":6030.1},
            {"from":{"line":1,"ch":0,"xRel":0},"to":{"line":1,"ch":4},"text":[""],"removed":["text"],"origin":"+delete","time":9039.635000000002},
            {"from":{"line":0,"ch":12},"to":{"line":1,"ch":0},"text":[""],"removed":["",""],"origin":"+delete","time":9404.6}
    ]}
];
