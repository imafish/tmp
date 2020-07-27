using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCoreTest
{
    class JsonString
    {
        const string schemaString = @"
{
	""$schema"":""http://json-schema.org/draft-07/schema#"",
	""$id"":""http://titanhq.com/schemas/v1.0.response.configutation.device.json"",
	""title"":""Device configutation"",
	""description"":""Device configuration response"",
	""type"":""object"",
	""definitions"":{
		""rpc_svr_type"":{
			""title"":""RPC server"",
			""description"":""The RPC server URL"",
			""type"":""string"",
			""anyOf"":[
				{
					""format"":""uri""
				},
				{
					""format"":""hostname""
				},
				{
					""format"":""ipv4""
				},
				{
					""format"":""ipv6""
				}
			],
			""examples"":[
				""rpc://wtc1.webtitancloud.com:7771"",
				""wtc1.webtitancloud.com"",
				""10.1.1.1"",
				""2001:470:1f1d:cc7:10:1:8:250""
			]
		},
		""device_key_type"":{
			""title"":""Device Identification Key"",
			""type"":""string"",
			""format"":""uuid"",
			""_alt_pattern"":""^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$""
		},
		""interval_type"":{
			""type"":""object"",
			""properties"":{
				""unit"":{
					""title"":""Unit"",
					""description"":""duration unit s[econds], m[inutes], h[ours], d[ays]"",
					""enum"":[
						""s"",
						""m"",
						""h"",
						""d""
					],
					""default"":""s""
				}
			},
			""allOf"":[
				{
					""if"":{
						""properties"":{
							""unit"":{
								""const"":""s""
							}
						}
					},
					""then"":{
						""properties"":{
							""value"":{
								""description"":""interval in seconds"",
								""type"":""integer"",
								""default"":5,
								""minimum"":5,
								""maximum"":86400
							}
						}
					}
				},
				{
					""if"":{
						""properties"":{
							""unit"":{
								""const"":""m""
							}
						}
					},
					""then"":{
						""properties"":{
							""value"":{
								""description"":""interval in minutes"",
								""type"":""integer"",
								""default"":5,
								""minimum"":5,
								""maximum"":1440
							}
						}
					}
				},
				{
					""if"":{
						""properties"":{
							""unit"":{
								""const"":""h""
							}
						}
					},
					""then"":{
						""properties"":{
							""value"":{
								""description"":""interval in hours"",
								""type"":""integer"",
								""default"":1,
								""minimum"":1,
								""maximum"":24
							}
						}
					}
				},
				{
					""if"":{
						""properties"":{
							""unit"":{
								""const"":""d""
							}
						}
					},
					""then"":{
						""properties"":{
							""value"":{
								""description"":""interval in days"",
								""type"":""integer"",
								""default"":1,
								""minimum"":1,
								""maximum"":7
							}
						}
					}
				}
			]
		}
	},
	""required"":[
		""jsonrpc"",
		""result""
	],
	""properties"":{
		""jsonrpc"":{
			""type"":""string""
		},
		""result"":{
			""type"":""object"",
			""required"":[
				""cfg""
			],
			""properties"":{
				""hmac"":{
					""type"":""string"",
					""title"":""HMAC"",
					""description"":""Optional HMAC""
				},
				""cfg"":{
					""title"":""Configuration"",
					""description"":""Configuration"",
					""type"":""object"",
					""required"":[
						""persistent"",
						""runtime""
					],
					""properties"":{
						""persistent"":{
							""title"":""Persistent"",
							""description"":""Persistent configuration: if send it should be identical to what is already in the persistent store (e.g. INI file)"",
							""type"":""object"",
							""required"":[
								""key""
							],
							""properties"":{
								""rpc"":{
									""$ref"":""#/definitions/rpc_svr_type""
								},
								""rpc_def_port"":{
									""title"":""Default RPC port"",
									""description"":""Default RPC port number if not specified otherwise"",
									""type"":""integer"",
									""default"":7771
								},
								""key"":{
									""$ref"":""#/definitions/device_key_type""
								}
							}
						},
						""runtime"":{
							""title"":""Runtime"",
							""description"":""Runtime configuration: this data cannot be permanently stored"",
							""type"":""object"",
							""required"":[
								""locations"",
								""filters""
							],
							""properties"":{
								""otg"":{
									""type"":""object"",
									""title"":""OTG"",
									""description"":""OTG specyfic options"",
									""properties"":{
										""root_ca"":{
											""title"":""Root CA"",
											""type"":""object"",
											""description"":""Use it to signal that new root CA cert is available for download"",
											""properties"":{
												""hash"":{
													""title"":""Hash"",
													""description"":""Hash of the root CA: when different from the previously installed it should trigger the download"",
													""type"":""string"",
													""pattern"":""^.*$"",
													""minLength"":64,
													""maxLength"":64
												},
												""url"":{
													""title"":""URL"",
													""description"":""Url to donwnload new root CA"",
													""type"":""string"",
													""format"":""uri""
												}
											}
										},
										""tray_type"":{
											""title"":""Tray Type"",
											""description"":""Controls the WebTitan Cloud OTG information, logo and popup notifications in the Windows system tray or macOS menu bar. Values can be: 0 - hide the tray icon; 1 - only show tool tip, but no notification balloon. (default);2 - show both tool tip and notification balloon."",
											""type"":""integer"",
											""default"":1,
											""minimum"":0,
											""maximum"":2
										},
										""test_mode"":{
											""title"":""Test mode"",
											""type"":""boolean"",
											""default"":true
										},
										""log_debug"":{
											""title"":""log debug"",
											""type"":""boolean"",
											""default"":false
										},
										""interfaces_to_ignore"":{
											""title"":""Interfaces to ignore"",
											""description"":""Ignore the network interfaces containing the following strings"",
											""type"":""array"",
											""default"":[
												""TAP"",
												""VPN"",
												""Fortinet"",
												""Juniper"",
												""Citrix Virtual Adapter"",
												""SonicWALL Virtual NIC"",
												""Cisco AnyConnect"",
												""PANGP""
											],
											""items"":{
												""type"":""string""
											},
											""examples"":[
												""TAP"",
												""VPN"",
												""Fortinet"",
												""Juniper"",
												""Citrix Virtual Adapter"",
												""SonicWALL Virtual NIC"",
												""Cisco AnyConnect"",
												""PANGP""
											]
										},
										""ip_probe"":{
											""title"":""IP probing"",
											""type"":""object"",
											""properties"":{
												""interval"":{
													""$ref"":""#/definitions/interval_type""
												},
												""timeout"":{
													""allOf"":[
														{
															""$ref"":""#/definitions/interval_type""
														},
														{
															""properties"":{
																""value"":{
																	""default"":10,
																	""maximum"":300
																}
															}
														}
													]
												}
											}
										},
										""internet_probe"":{
											""title"":""Internet probing"",
											""type"":""object"",
											""properties"":{
												""interval"":{
													""$ref"":""#/definitions/interval_type""
												},
												""timeout"":{
													""allOf"":[
														{
															""$ref"":""#/definitions/interval_type""
														},
														{
															""properties"":{
																""value"":{
																	""default"":10,
																	""maximum"":300
																}
															}
														}
													]
												}
											}
										}
									}
								},
								""reconfigure"":{
									""properties"":{
										""interval"":{
											""allOf"":[
												{
													""$ref"":""#/definitions/interval_type""
												},
												{
													""properties"":{
														""unit"":{
															""default"":""h""
														}
													}
												}
											]
										}
									}
								},
								""locations"":{
									""title"":""Locations tags"",
									""type"":""array"",
									""uniqueItems"":true,
									""items"":{
										""type"":""object"",
										""required"":[
											""tag""
										],
										""properties"":{
											""name"":{
												""type"":""string"",
												""description"":""The location name will be returned only if the location name was generated in the backend due to the presence of the use_dev_name flag in the configuration request."",
												""default"":"""",
												""pattern"":""^.*$"",
												""minLength"":1,
												""maxLength"":250
											},
											""tag"":{
												""type"":""string"",
												""pattern"":""^.*$"",
												""minLength"":1,
												""maxLength"":64
											},
											""rule"":{
												""type"":""string"",
												""default"":"""",
												""pattern"":""^!?[a-f0-9.:\/]+$"",
												""maxLength"":64
											}
										}
									}
								},
								""user"":{
									""title"":""User"",
									""description"":""User's G/UUID"",
									""type"":""string"",
									""format"":""uuid""
								},
								""actions"":{
									""title"":""Actions"",
									""type"":""object"",
									""default"":{

									},
									""properties"":{
										""move"":{
											""title"":""migrate to the new RPC server"",
											""type"":""object"",
											""required"":[
												""rpc"",
												""key""
											],
											""properties"":{
												""rpc"":{
													""$ref"":""#/definitions/rpc_svr_type""
												},
												""key"":{
													""$ref"":""#/definitions/device_key_type""
												},
												""location"":{
													""title"":""Location"",
													""description"":""Override Location name and/or tag to overcome potential location collision"",
													""type"":""object"",
													""properties"":{
														""name"":{
															""title"":""Name"",
															""description"":""Location name (optional)"",
															""type"":""string"",
															""default"":"""",
															""pattern"":""^.*$"",
															""minLength"":1,
															""maxLength"":250
														},
														""tag"":{
															""title"":""Tag"",
															""description"":""Location tag (optional)"",
															""type"":""string"",
															""default"":"""",
															""pattern"":""^.*$"",
															""minLength"":1,
															""maxLength"":64
														}
													}
												}
											}
										},
										""uninstall"":{
											""title"":""Uninstall"",
											""type"":""boolean"",
											""default"":false
										},
										""disable"":{
											""title"":""Disable"",
											""type"":""boolean"",
											""default"":false
										},
										""upgrade"":{
											""title"":""Upgrade"",
											""type"":""object"",
											""required"":[
												""url""
											],
											""properties"":{
												""url"":{
													""title"":""Upgrade url"",
													""type"":""string"",
													""format"":""uri""
												},
												""args"":{
													""title"":""Upgrade args"",
													""type"":""string""
												}
											}
										}
									},
									""oneOf"":[
										{
											""type"":""object"",
											""required"":[
												""move""
											]
										},
										{
											""type"":""object"",
											""required"":[
												""uninstall""
											]
										},
										{
											""type"":""object"",
											""required"":[
												""disable""
											]
										},
										{
											""type"":""object"",
											""required"":[
												""upgrade""
											]
										}
									]
								},
								""filters"":{
									""title"":""Filters"",
									""type"":""array"",
									""uniqueItems"":true,
									""minItems"":1,
									""examples"":[
										{
											""description"":""default entry, catch-all, essentially instructs where to send majority of the DNS traffic (IPs of the WTC DNS servers)"",
											""resolvers"":[
												""IP""
											]
										},
										{
											""description"":""conditional forwarding: If at this location, forward traffic to the specified resolvers"",
											""location"":""cidr/host"",
											""resolvers"":[
												""IP""
											]
										},
										{
											""description"":""conditional forwarding: If at this location, forward traffic to the system resolvers"",
											""location"":""cidr/host"",
											""resolvers"":[

											]
										},
										{
											""description"":""conditional forwarding: If at this location, forward traffic referring to the specified domain to the specified resolvers"",
											""location"":""cidr/host"",
											""domain"":""fqdn"",
											""resolvers"":[
												""IP""
											]
										},
										{
											""description"":""conditional forwarding: If at this location, forward traffic referring to the specified domain to the system resolvers"",
											""location"":""cidr/host"",
											""domain"":""fqdn"",
											""resolvers"":[

											]
										},
										{
											""description"":""conditional forwarding: at any location, forward traffic referring to the specified domain to the specified resolvers"",
											""domain"":""fqdn"",
											""resolvers"":[
												""IP""
											]
										},
										{
											""description"":""conditional forwarding: at any location, forward traffic referring to the specified domain to the system resolvers"",
											""domain"":""fqdn"",
											""resolvers"":[

											]
										}
									],
									""items"":{
										""type"":""object"",
										""title"":""forwarding rule"",
										""properties"":{
											""resolvers"":{
												""type"":""array"",
												""uniqueItems"":true,
												""title"":""Resolvers"",
												""examples"":[
													""8.8.8.8"",
													""9.9.9.9""
												],
												""default"":[

												],
												""items"":{
													""type"":""string"",
													""pattern"":""^[a-f0-9.:/]+$"",
													""maxLength"":64
												}
											},
											""domain"":{
												""type"":""string"",
												""format"":""hostname"",
												""title"":""FQDN"",
												""default"":"""",
												""maxLength"":256,
												""examples"":[
													""10.1.0.1""
												]
											},
											""location"":{
												""type"":""string"",
												""pattern"":""^!?|[a-f0-9.:\/]+$"",
												""title"":""The Location ADDRESS"",
												""default"":"""",
												""maxLength"":64,
												""examples"":[
													""10.1.0.1"",
													""10.1.0.0/16"",
													""!10.1.1.0/24""
												]
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
";

        const string jsonString = @"
{
    ""jsonrpc"":""2.0"",
    ""result"": {
        ""cfg"": {
            ""persistent"": {
                ""key"": ""123""
            },
            ""runtime"": {
                ""locations"": [
                    {
                        ""name"": ""location name"",
                        ""tag"": ""location tag""
                    }
                ],
                ""user"": ""ssdfasdf"",
                ""filters"": [
                    {
					    ""description"":""default entry, catch-all, essentially instructs where to send majority of the DNS traffic (IPs of the WTC DNS servers)"",
					    ""resolvers"":[
						    ""2.2.2.2""
					    ]
                    }
                ]
            }
        }
    }
}
";

        const string schemaString2 = @"
{
    ""title"": ""Album Options"",
    ""type"": ""object"",
    ""properties"": {
        ""root"": {
            ""type"":""object"",
            ""properties"": {
                ""sort"": {
                        ""type"" : ""array"",
                        ""items"": {
                            ""type"":""string""
                        }
                    },
                ""per_page"" : {
                        ""default"" : 30,
                        ""type"": ""integer""
                }
            }
        }
    }
}
";

        const string jsonString2 = @"
{
    ""root"": {
    }
}
";

    }
}
