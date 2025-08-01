INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'A3188301-5AD3-4629-A8E9-0B7AD6148FF8', N'Quality in software development', N'ICT.DT.QSD.20', N'Quality voor software ontwikkeling', N'{
    "SemesterConstraint": 0,
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "8EC4B98E-5BEB-433D-A74F-23ACEC3CB8BF"
                }
            ]
        }
    ],
    "AvailableFromYear": 3
}', 30, 3, N'5FEA0113-0F2F-4CA7-91A1-C9A854239666', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'EEF1AD70-1175-438C-A401-11DC5F4C024E', N'Afstuderen IDS', N'ICT.DT.IDS.20', N'Afstuderen IDS', N'{
    "Propaedeutic": true,
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "0D5E1FF3-D5C4-412D-B675-32077E3148B1",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "9E0B3656-A60A-426A-B0C6-7FB073788178",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "01581451-3A01-4CC8-B8C4-DA0D18277D9F",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        },
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "0D5E1FF3-D5C4-412D-B675-32077E3148B1",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "9E0B3656-A60A-426A-B0C6-7FB073788178",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "BDBF1FF0-4F24-46F5-905B-D0FB3C86240C",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "ModuleLevelRequirementGroups": [
        {
            "ModuleLevelRequirements": [
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "AvailableFromYear": 3
}', 30, 4, N'51B9D124-AF17-4D8A-934C-A8A35BC0CC1A', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'8EC4B98E-5BEB-433D-A74F-23ACEC3CB8BF', N'OO software design & development', N'ICT.DT.OOSDD.20', N'OOP the basics, maar dan ook echt heel basic...', N'{
    "SemesterConstraint": 0,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'5FEA0113-0F2F-4CA7-91A1-C9A854239666', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'F70E2A6D-24CA-4CA2-9E42-2F21F77FE898', N'Business Process Management ', N'ICT.DT.BPM.20', N'Business en process management', N'{
    "SemesterConstraint": 0,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'932172FF-166D-496C-9753-961A663A67AB', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'B8FDCE68-1C43-4C29-BCD1-2FAE9327D845', N'Beheren van een verandertraject', N'ICT.DT.BV.20', N'Beheren van een verandertraject', N'{
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "FC9ABCA1-4C46-4696-8409-DC18C368046B"
                }
            ]
        }
    ],
    "YearConstraints": [
        1
    ]
}', 30, 1, N'9E6EDD59-EB6E-4DBC-8947-C0AA61B0CE2E', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 1);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'0D5E1FF3-D5C4-412D-B675-32077E3148B1', N'Multidisciplinaire Opdracht', N'ICT.DT.MDO.20', N'MDO erg belangrijk', N'{
    "ModuleLevelRequirementGroups": [
        {
            "ModuleLevelRequirements": [
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "AvailableFromYear": 3
}', 30, 3, N'9E6EDD59-EB6E-4DBC-8947-C0AA61B0CE2E', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'46CCFC59-7B23-4F62-9930-562D9E18B09B', N'Data science ', N'ICT.DT.DS.20', N'Data science in business', N'{
    "SemesterConstraint": 1,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'932172FF-166D-496C-9753-961A663A67AB', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'7AE818C2-BE4F-4C53-B407-74C744894BEA', N'Management of IT', N'ICT.DT.MI.20', N'Management voor de it wereld', N'{
    "SemesterConstraint": 0,
    "AvailableFromYear": 3
}', 30, 3, N'932172FF-166D-496C-9753-961A663A67AB', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'9E0B3656-A60A-426A-B0C6-7FB073788178', N'Hybride cloud en infrastructuur', N'ICT.DT.HCI.20', N'Cloud en infra', N'{
    "SemesterConstraint": 0,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'51B9D124-AF17-4D8A-934C-A8A35BC0CC1A', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'8FB5EF25-DD1C-43ED-836B-AFCA3D34865D', N'Afstuderen BIM', N'ICT.DT.BIM.20', N'Afstuderen BIM', N'{
    "Propaedeutic": true,
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "0D5E1FF3-D5C4-412D-B675-32077E3148B1",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "F70E2A6D-24CA-4CA2-9E42-2F21F77FE898",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "46CCFC59-7B23-4F62-9930-562D9E18B09B",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "ModuleLevelRequirementGroups": [
        {
            "ModuleLevelRequirements": [
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "AvailableFromYear": 3
}', 30, 4, N'932172FF-166D-496C-9753-961A663A67AB', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'BDBF1FF0-4F24-46F5-905B-D0FB3C86240C', N'Applied IT Security ', N'ICT.DT.AIS.20', N'Security in IT', N'{
    "SemesterConstraint": 0,
    "AvailableFromYear": 3
}', 30, 3, N'51B9D124-AF17-4D8A-934C-A8A35BC0CC1A', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'01581451-3A01-4CC8-B8C4-DA0D18277D9F', N'Cloud Architecture and Automation ', N'ICT.DT.CAA.20', N'Cloud automation', N'{
    "SemesterConstraint": 1,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'51B9D124-AF17-4D8A-934C-A8A35BC0CC1A', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'FC9ABCA1-4C46-4696-8409-DC18C368046B', N'Bedrijfsprocessen en dynamische webapplicaties', N'ICT.DT.BDW.20', N'Introductie module voor ict.', N'{
    "YearConstraints": [
        1
    ]
}', 30, 1, N'9E6EDD59-EB6E-4DBC-8947-C0AA61B0CE2E', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 1, 0, 1);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'7955B362-2B2C-4881-A194-E4033FC49ADB', N'Afstuderen SE', N'ICT.DT.SE.20', N'Afstuderen SE', N'{
    "Propaedeutic": true,
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "0D5E1FF3-D5C4-412D-B675-32077E3148B1",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "8EC4B98E-5BEB-433D-A74F-23ACEC3CB8BF",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "1B5D7E3A-77A9-4983-9534-E678FEDC79D8",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        },
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "0D5E1FF3-D5C4-412D-B675-32077E3148B1",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "8EC4B98E-5BEB-433D-A74F-23ACEC3CB8BF",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "RelevantModuleId": "A3188301-5AD3-4629-A8E9-0B7AD6148FF8",
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "ModuleLevelRequirementGroups": [
        {
            "ModuleLevelRequirements": [
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                },
                {
                    "Level": 2,
                    "EcRequirement": {
                        "RequiredAmount": 30,
                        "Propaedeutic": false
                    }
                }
            ]
        }
    ],
    "AvailableFromYear": 3
}', 30, 4, N'5FEA0113-0F2F-4CA7-91A1-C9A854239666', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'1B5D7E3A-77A9-4983-9534-E678FEDC79D8', N'Web development', N'ICT.DT.WD.20', N'Web dev module', N'{
    "SemesterConstraint": 1,
    "EcRequirements": [
        {
            "RequiredAmount": 50,
            "Propaedeutic": true
        }
    ],
    "ModuleRequirementGroups": [
        {
            "ModuleRequirements": [
                {
                    "RelevantModuleId": "8EC4B98E-5BEB-433D-A74F-23ACEC3CB8BF"
                }
            ]
        }
    ],
    "AvailableFromYear": 2
}', 30, 2, N'5FEA0113-0F2F-4CA7-91A1-C9A854239666', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
INSERT INTO HboIctKeuzewijzer.dbo.Modules (Id, Name, Code, Description, PrerequisiteJson, ECs, Level, CategoryId, OerId, Required, RequiredSemester, IsPropaedeutic) VALUES (N'CEFA9348-A491-49E2-A7D8-E82EA2F1FAF8', N'Stage', N'ICT.DT.ST.20', N'Stage', N'{
    "AvailableFromYear": 3
}', 30, 3, N'9E6EDD59-EB6E-4DBC-8947-C0AA61B0CE2E', N'A09AF7C8-7DCD-4BD2-B2B9-9CCFBCE92245', 0, null, 0);
